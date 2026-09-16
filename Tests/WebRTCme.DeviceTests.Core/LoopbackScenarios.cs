using System.Diagnostics;
using System.Text;
using Microsoft.JSInterop;

namespace WebRTCme.DeviceTests.Core;

/// <summary>
/// Two peer connections in one process, negotiating with each other.
/// </summary>
/// <remarks>
/// <para>
/// The checks that prove the package works rather than merely restores. Everything in tiers 1 and 2
/// is managed code: none of it loads libwebrtc.aar, WebRTC.xcframework, WebRTC.framework or
/// WebRtcInterop.dll, so a package whose native payload is missing or built for the wrong
/// architecture passes all of them and fails here.
/// </para>
/// <para>
/// No signalling server, no second machine, no network and no camera. The offer, the answer and the
/// ICE candidates are handed between the two connections in code, which is all signalling ever does.
/// What is left is exactly the part that can fail without a person watching: the native library
/// loading, SDP marshalling both ways, candidates surviving the round trip, and DTLS and SCTP
/// completing.
/// </para>
/// <para>
/// Each scenario returns a result rather than throwing, so the device runner can report a list
/// without hosting a test framework. The Windows xUnit project wraps each one in a [Fact].
/// </para>
/// </remarks>
public static class LoopbackScenarios
{
    // Long enough for DTLS on a slow phone, short enough that a hang is a failure rather than a
    // coffee break. Host candidates over loopback are immediate; this is nearly all handshake.
    static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    /// <summary>Every scenario, in the order a runner should execute them.</summary>
    public static IReadOnlyList<(string Name, Func<Task<ScenarioResult>> Run)> All =>
    [
        (nameof(NativeLibraryLoads), NativeLibraryLoads),
        (nameof(OfferIsRealSdp), OfferIsRealSdp),
        (nameof(TwoPeersNegotiateAndCarryAMessage), TwoPeersNegotiateAndCarryAMessage),
        (nameof(DevicesEnumerateWhateverElseHasHappened), DevicesEnumerateWhateverElseHasHappened),
        (nameof(ACompletedCallDoesNotChangeWhatDevicesExist), ACompletedCallDoesNotChangeWhatDevicesExist),
    ];

    static RTCConfiguration Configuration() => new()
    {
        // No STUN server. Loopback needs only host candidates, and reaching for a public one would
        // make this depend on the internet being up.
        IceServers = [],
        IceTransportPolicy = RTCIceTransportPolicy.All
    };

    /// <summary>
    /// The browser's JS runtime, on Blazor. Null everywhere else, and ignored there.
    /// </summary>
    /// <remarks>
    /// Blazor is the one binding that is not a native library: it is JSInterop over the browser's
    /// own WebRTC API, so it needs the runtime handle that a component gets injected and a plain
    /// class has no way to reach. IWebRtc.Window takes it as an optional argument for exactly this
    /// reason, so passing it unconditionally is correct on every platform - the other four bindings
    /// take the argument and ignore it.
    /// </remarks>
    public static IJSRuntime? JsRuntime { get; set; }

    static IWindow Window() => CrossWebRtc.Current.Window(JsRuntime);

    /// <summary>
    /// The three fields a candidate needs to be re-added at the other end. The real signalling path
    /// serialises the same record to JSON and back; here it is handed over directly.
    /// </summary>
    static RTCIceCandidateInit Init(IRTCIceCandidate candidate) => new()
    {
        Candidate = candidate.Candidate,
        SdpMid = candidate.SdpMid,
        SdpMLineIndex = candidate.SdpMLineIndex
    };

    /// <summary>
    /// How long any one scenario may take before it is called a failure.
    /// </summary>
    /// <remarks>
    /// Generous - the slowest passing scenario observed anywhere is under two seconds, on an
    /// emulator - because this exists to catch a hang, not to police speed.
    /// </remarks>
    static readonly TimeSpan ScenarioTimeout = TimeSpan.FromSeconds(60);

    /// <summary>Runs one scenario, timing it and turning any exception into a failed result.</summary>
    /// <remarks>
    /// <para>
    /// Bounded, and that is not a nicety. Until it was, a scenario that blocked took the whole run
    /// with it: no summary line, no name, nothing to say which of the five stopped. A Windows CI
    /// runner produced no output at all for six hours that way, and an Android emulator ran four
    /// scenarios and then went quiet. Neither told anyone anything.
    /// </para>
    /// <para>
    /// The blocked work is not cancelled, because nothing here can be: these are awaits on native
    /// callbacks that never arrive. It is abandoned, the scenario is reported failed, and the run
    /// carries on - so the remaining scenarios still say something, and the one that hung is named.
    /// A run that continues after this is worth less than a clean one, and the message says so.
    /// </para>
    /// </remarks>
    static async Task<ScenarioResult> Run(string name, Func<Task<string?>> body)
    {
        var clock = Stopwatch.StartNew();
        try
        {
            var work = body();
            var finished = await Task.WhenAny(work, Task.Delay(ScenarioTimeout));

            if (!ReferenceEquals(finished, work))
            {
                clock.Stop();
                return ScenarioResult.Fail(
                    name,
                    $"timed out after {ScenarioTimeout.TotalSeconds:F0}s - it is still blocked, and "
                    + "anything reported after this ran alongside it",
                    clock.Elapsed);
            }

            var failure = await work;
            clock.Stop();
            return failure is null
                ? ScenarioResult.Pass(name, clock.Elapsed)
                : ScenarioResult.Fail(name, failure, clock.Elapsed);
        }
        catch (SkipException skip)
        {
            clock.Stop();
            return ScenarioResult.Skip(name, skip.Message, clock.Elapsed);
        }
        catch (Exception exception)
        {
            clock.Stop();
            // The type matters as much as the message here: DllNotFoundException means the native
            // payload is missing from the package, BadImageFormatException means it is there and
            // built for the wrong architecture.
            //
            // The top of the stack goes out too. A device log is all there is here - no debugger
            // attached, no test framework catching this - and "NullReferenceException" on its own
            // names neither the layer that threw nor the call that reached it.
            return ScenarioResult.Fail(name, $"{exception.GetType().Name}: {exception.Message}{Where(exception)}", clock.Elapsed);
        }
    }

    /// <summary>
    /// The first few stack frames of an exception, as one line. Trimmed because this has to fit in a
    /// log line, and because the frames that identify a fault are the ones nearest it.
    /// </summary>
    static string Where(Exception exception)
    {
        var frames = (exception.StackTrace ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(6)
            .ToArray();

        return frames.Length == 0 ? string.Empty : " << " + string.Join(" << ", frames);
    }

    sealed class SkipException(string message) : Exception(message);

    static async Task<bool> Within<T>(TaskCompletionSource<T> source)
    {
        var finished = await Task.WhenAny(source.Task, Task.Delay(Patience));
        return ReferenceEquals(finished, source.Task);
    }

    /// <summary>
    /// The cheapest possible proof that the native half is present and callable. If the package is
    /// missing its native payload this throws before anything interesting is attempted.
    /// </summary>
    public static Task<ScenarioResult> NativeLibraryLoads() =>
        Run(nameof(NativeLibraryLoads), () =>
        {
            using var pc = Window().RTCPeerConnection(Configuration());

            if (pc.SignalingState != RTCSignalingState.Stable)
                return Task.FromResult<string?>($"a new peer connection should be Stable, was {pc.SignalingState}");
            if (pc.ConnectionState != RTCPeerConnectionState.New)
                return Task.FromResult<string?>($"a new peer connection should be New, was {pc.ConnectionState}");

            return Task.FromResult<string?>(null);
        });

    public static Task<ScenarioResult> OfferIsRealSdp() =>
        Run(nameof(OfferIsRealSdp), async () =>
        {
            using var pc = Window().RTCPeerConnection(Configuration());
            pc.CreateDataChannel("probe");

            var offer = await pc.CreateOffer();

            if (offer.Type != RTCSdpType.Offer) return $"expected an offer, got {offer.Type}";
            if (offer.Sdp is null || !offer.Sdp.StartsWith("v=0")) return "an SDP blob begins with its version line";
            if (!offer.Sdp.Contains("m=application")) return "the data channel should have produced an m-line";

            return null;
        });

    /// <summary>The whole negotiation. Everything a call does before media flows happens here.</summary>
    public static Task<ScenarioResult> TwoPeersNegotiateAndCarryAMessage() =>
        Run(nameof(TwoPeersNegotiateAndCarryAMessage), async () =>
        {
            using var caller = Window().RTCPeerConnection(Configuration());
            using var callee = Window().RTCPeerConnection(Configuration());

            // Signalling, in two handlers - all a signalling server ever does with candidates,
            // minus the JSON and the websocket.
            caller.OnIceCandidate += async (_, e) =>
            {
                if (e.Candidate is not null) await callee.AddIceCandidate(Init(e.Candidate));
            };
            callee.OnIceCandidate += async (_, e) =>
            {
                if (e.Candidate is not null) await caller.AddIceCandidate(Init(e.Candidate));
            };

            var received = new TaskCompletionSource<string>();
            var calleeChannelOpen = new TaskCompletionSource<bool>();

            callee.OnDataChannel += (_, e) =>
            {
                var incoming = e.Channel;
                incoming.OnMessage += (_, m) => received.TrySetResult(
                    m.Data as string ?? Encoding.UTF8.GetString((byte[])m.Data));

                // ReadyState first, and this is not defensive padding - waiting only on OnOpen
                // hangs. The native observer is registered when the wrapper is built, on the
                // callback thread, while this handler is posted to the dispatcher. The channel
                // routinely opens in the gap, so OnOpen has already fired before anyone is
                // subscribed. Same shape as the browser API, and the same fix.
                if (incoming.ReadyState == RTCDataChannelState.Open) calleeChannelOpen.TrySetResult(true);
                else incoming.OnOpen += (_, _) => calleeChannelOpen.TrySetResult(true);
            };

            var connected = new TaskCompletionSource<bool>();
            caller.OnConnectionStateChanged += (_, _) =>
            {
                if (caller.ConnectionState == RTCPeerConnectionState.Connected) connected.TrySetResult(true);
                if (caller.ConnectionState == RTCPeerConnectionState.Failed)
                    connected.TrySetException(new Exception("the caller's connection failed"));
            };

            var channel = caller.CreateDataChannel("loopback");
            var callerChannelOpen = new TaskCompletionSource<bool>();
            channel.OnOpen += (_, _) => callerChannelOpen.TrySetResult(true);
            if (channel.ReadyState == RTCDataChannelState.Open) callerChannelOpen.TrySetResult(true);

            var offer = await caller.CreateOffer();
            await caller.SetLocalDescription(offer);
            await callee.SetRemoteDescription(offer);

            var answer = await callee.CreateAnswer();
            await callee.SetLocalDescription(answer);
            await caller.SetRemoteDescription(answer);

            if (!await Within(connected)) return $"the peer connection did not connect within {Patience.TotalSeconds:0}s";
            if (!await Within(callerChannelOpen)) return "the caller's data channel never opened";
            if (!await Within(calleeChannelOpen)) return "the callee's data channel never opened";

            const string message = "hello from the other side";
            channel.Send(message);

            if (!await Within(received)) return "the message never arrived";
            var got = await received.Task;
            if (got != message) return $"expected '{message}', got '{got}'";

            return null;
        });

    /// <summary>
    /// Enumeration answers "what can I use?", and one kind of device being uncountable must not
    /// turn the whole answer into an exception. On Windows a completed call used to leave the audio
    /// device module unable to count inputs, and that threw out of EnumerateDevices carrying the
    /// cameras with it.
    /// </summary>
    public static Task<ScenarioResult> DevicesEnumerateWhateverElseHasHappened() =>
        Run(nameof(DevicesEnumerateWhateverElseHasHappened), async () =>
        {
            await Window().Navigator().MediaDevices.EnumerateDevices();
            return null;
        });

    /// <summary>
    /// A call must not change what devices the machine appears to have.
    /// </summary>
    /// <remarks>
    /// Needs a process in which no call has happened yet, and checks that rather than assuming it.
    /// The damage it looks for is process-global: if a negotiation has already run, the baseline is
    /// the already-broken one and "unchanged" holds trivially. Written without that guard, this
    /// passed against the very build it was meant to catch.
    /// </remarks>
    public static Task<ScenarioResult> ACompletedCallDoesNotChangeWhatDevicesExist() =>
        Run(nameof(ACompletedCallDoesNotChangeWhatDevicesExist), async () =>
        {
            var devices = Window().Navigator().MediaDevices;

            var before = await devices.EnumerateDevices();
            var cameras = before.Count(d => d.Kind == MediaDeviceInfoKind.VideoInput);
            var microphones = before.Count(d => d.Kind == MediaDeviceInfoKind.AudioInput);

            if (cameras == 0 && microphones == 0)
            {
                if (Environment.GetEnvironmentVariable("WEBRTCME_TESTS_REQUIRED") == "1")
                    return "no capture devices, and WEBRTCME_TESTS_REQUIRED=1 says that is a failure";
                throw new SkipException("no capture devices attached, so there is nothing for a call to hide");
            }

            // The guard that stops this passing for the wrong reason.
            if (cameras > 0 && microphones == 0)
                return "baseline shows cameras but no microphones - either this machine has none, "
                     + "or a call has already run in this process and broken the audio device module, "
                     + "in which case this must run in a process of its own";

            using (var caller = Window().RTCPeerConnection(Configuration()))
            using (var callee = Window().RTCPeerConnection(Configuration()))
            {
                caller.CreateDataChannel("probe");
                var offer = await caller.CreateOffer();
                await caller.SetLocalDescription(offer);
                await callee.SetRemoteDescription(offer);
                var answer = await callee.CreateAnswer();
                await callee.SetLocalDescription(answer);
                await caller.SetRemoteDescription(answer);
            }

            var after = await devices.EnumerateDevices();
            var camerasAfter = after.Count(d => d.Kind == MediaDeviceInfoKind.VideoInput);
            var microphonesAfter = after.Count(d => d.Kind == MediaDeviceInfoKind.AudioInput);

            if (camerasAfter != cameras) return $"cameras went from {cameras} to {camerasAfter} across a call";
            if (microphonesAfter != microphones) return $"microphones went from {microphones} to {microphonesAfter} across a call";

            return null;
        });
}
