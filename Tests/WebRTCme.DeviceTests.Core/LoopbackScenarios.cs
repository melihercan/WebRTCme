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

    // How long AReceivedTrackDoesNotPoisonTheProcess waits for a track before carrying on without
    // one. Short on purpose: whether the track arrives is not what that scenario asserts.
    static readonly TimeSpan TrackArrival = TimeSpan.FromSeconds(3);

    /// <summary>Every scenario, in the order a runner should execute them.</summary>
    /// <remarks>
    /// The order is load-bearing, and not for the usual reason. Receiving a track leaves libwebrtc
    /// holding something a finalizer will free underneath it - issue #45 - and the process then
    /// dies on the next call into libwebrtc, wherever that happens to be. While the two scenarios
    /// that receive a track ran in the middle, the crash landed on whichever innocent scenario came
    /// after them, or on none of them if no collection happened in the window. An Android job that
    /// passes on one run and dies on the next, on identical code, teaches nobody anything.
    ///
    /// So everything that does not receive a track runs first and reports honestly, and the two
    /// that do are last. ACompletedCallDoesNotChangeWhatDevicesExist still has a completed call
    /// behind it - TwoPeersNegotiateAndCarryAMessage - which is all it needs. That much worked:
    /// those two scenarios now report on every platform instead of being collateral damage.
    ///
    /// What it did not do is make the failure reliable - see AReceivedTrackDoesNotPoisonTheProcess,
    /// which says so at length. The Android job is still one whose green means nothing.
    /// </remarks>
    public static IReadOnlyList<(string Name, Func<Task<ScenarioResult>> Run)> All =>
    [
        (nameof(NativeLibraryLoads), NativeLibraryLoads),
        (nameof(OfferIsRealSdp), OfferIsRealSdp),
        (nameof(TwoPeersNegotiateAndCarryAMessage), TwoPeersNegotiateAndCarryAMessage),
        (nameof(DevicesEnumerateWhateverElseHasHappened), DevicesEnumerateWhateverElseHasHappened),
        (nameof(ACompletedCallDoesNotChangeWhatDevicesExist), ACompletedCallDoesNotChangeWhatDevicesExist),

        // Last, and in this order: the first receives a track, the second forces the collection
        // that turns #45 from a race into a certainty.
        (nameof(ARemoteTrackArrivesWithoutCrashing), ARemoteTrackArrivesWithoutCrashing),
        (nameof(AReceivedTrackDoesNotPoisonTheProcess), AReceivedTrackDoesNotPoisonTheProcess),
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

    /// <summary>
    /// The platform's window, with the first call traced.
    /// </summary>
    /// <remarks>
    /// The first call is the expensive one and the one that can hang. On Windows it resolves
    /// WebRtcRuntime.Factory, which calls Initialize() and FactoryCreate() as two synchronous
    /// P/Invokes, and FactoryCreate builds libwebrtc's audio device module. A machine with no audio
    /// endpoints at all - a hosted Windows Server runner has none - is the one place this has ever
    /// failed to return, so the two lines around it are what say whether it did.
    /// </remarks>
    static IWindow Window()
    {
        if (_windowTraced) return CrossWebRtc.Current.Window(JsRuntime);

        _windowTraced = true;
        ScenarioTrace.Write("creating the first window - this initialises the native factory");
        var window = CrossWebRtc.Current.Window(JsRuntime);
        ScenarioTrace.Write("first window created");
        return window;
    }

    static bool _windowTraced;

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
            // Task.Run, not body() directly, and that is the difference between a timeout that
            // works and one that only looks like it does. Several scenarios are synchronous up to
            // their first await - NativeLibraryLoads is synchronous throughout - so a native call
            // that blocks blocks the calling thread, Task.WhenAny below is never reached, and the
            // timeout never fires. Starting the work on a pool thread means a synchronous block is
            // caught by exactly the same mechanism as an await that never completes.
            ScenarioTrace.Write($"begin {name}");

            var work = Task.Run(body);
            var finished = await Task.WhenAny(work, Task.Delay(ScenarioTimeout));

            if (!ReferenceEquals(finished, work))
            {
                clock.Stop();
                ScenarioTrace.Write($"TIMED OUT {name} - still blocked");
                return ScenarioResult.Fail(
                    name,
                    $"timed out after {ScenarioTimeout.TotalSeconds:F0}s - it is still blocked, and "
                    + "anything reported after this ran alongside it",
                    clock.Elapsed);
            }

            var failure = await work;
            clock.Stop();
            ScenarioTrace.Write($"end {name} after {clock.ElapsedMilliseconds}ms");
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

    /// <summary>A remote track arriving must not take the process with it.</summary>
    /// <remarks>
    /// <para>
    /// Every other scenario negotiates a data channel, so nothing here ever received media and
    /// the platform's onTrack callback was never reached. On Android that callback crashed the
    /// app: the generated binding declares onTrack as a default interface method whose body
    /// calls back into Java, so leaving it unimplemented sent libwebrtc's call straight into the
    /// abstract method it came from - AbstractMethodError, fatal, a few seconds into any call
    /// carrying audio or video. Reported as issue #35 and invisible to tiers 1 and 2, because
    /// nothing below this one loads the native SDK.
    /// </para>
    /// <para>
    /// A send-only transceiver rather than getUserMedia, so this needs no camera, no microphone
    /// and no permission prompt: what fires onTrack is the callee's remote description gaining
    /// an m-line it will receive on, and that costs nothing to arrange. There is no ICE exchange
    /// and no connection either - SetRemoteDescription is the whole trigger.
    /// </para>
    /// </remarks>
    public static Task<ScenarioResult> ARemoteTrackArrivesWithoutCrashing() =>
        Run(nameof(ARemoteTrackArrivesWithoutCrashing), async () =>
        {
            using var caller = Window().RTCPeerConnection(Configuration());
            using var callee = Window().RTCPeerConnection(Configuration());

            var tracked = new TaskCompletionSource<bool>();
            callee.OnTrack += (_, _) => tracked.TrySetResult(true);

            caller.AddTransceiver(MediaStreamTrackKind.Audio,
                new RTCRtpTransceiverInit { Direction = RTCRtpTransceiverDirection.SendOnly });

            var offer = await caller.CreateOffer();
            await caller.SetLocalDescription(offer);
            if (offer.Sdp is null || !offer.Sdp.Contains("m=audio"))
                return "the send-only transceiver should have produced an audio m-line to receive on";

            await callee.SetRemoteDescription(offer);

            if (!await Within(tracked))
                return "the callee never raised OnTrack for the remote audio m-line";

            return null;
        });

    /// <summary>Receiving a track must not leave libwebrtc unusable afterwards.</summary>
    /// <remarks>
    /// <para>
    /// Currently fails on Android, and is meant to. Reading a receiver's track inside the track
    /// callback - <c>p0.Track()</c> there, <c>rtpReceiver.Track</c> on Apple - makes a wrapper over
    /// a native object the app does not own, and finalizing that wrapper frees something libwebrtc
    /// is still holding. The next call into libwebrtc faults at offset 0x30 on one of its own
    /// threads, with no managed frames and nothing to catch. That is issue #45.
    /// </para>
    /// <para>
    /// <b>This does not fire reliably, and the forced collection was not enough to make it.</b> It
    /// was written to convert a race into a certainty and it does not: CI has run the whole suite
    /// green on Android with this scenario in place, on code that reproduces the fault elsewhere.
    /// Treat a green Android run as saying nothing, exactly as #45 does. What this scenario buys is
    /// that when the fault does fire it fires here, with a name, instead of killing whichever
    /// unrelated scenario happened to come next.
    /// </para>
    /// <para>
    /// There is a plausible reason it got <em>less</em> likely rather than more, and it is worth
    /// knowing before someone tries again: moving every other scenario ahead of the track read
    /// also removed the calls into libwebrtc that the poisoned process used to die on. What is left
    /// afterwards is one canary. Making this reliable probably means giving the fault more to land
    /// on, not more collections - but that is a guess, and the last one was wrong.
    /// </para>
    /// <para>
    /// The canary at the end is the call that actually dies. The damage is done by the finalizer,
    /// but the crash lands on whatever touches libwebrtc next, which is why it kept surfacing in
    /// unrelated scenarios and sent several investigations after the wrong thing entirely.
    /// </para>
    /// <para>
    /// Platforms where OnTrack never fires do not read a track and so pass this, which is accurate
    /// rather than lucky: they are not exposed to #45 because the feature that exposes it does not
    /// work. See <c>ARemoteTrackArrivesWithoutCrashing</c>, which is the one that says so.
    /// </para>
    /// </remarks>
    public static Task<ScenarioResult> AReceivedTrackDoesNotPoisonTheProcess() =>
        Run(nameof(AReceivedTrackDoesNotPoisonTheProcess), async () =>
        {
            var caller = Window().RTCPeerConnection(Configuration());
            var callee = Window().RTCPeerConnection(Configuration());

            var tracked = new TaskCompletionSource<bool>();
            callee.OnTrack += (_, _) => tracked.TrySetResult(true);

            caller.AddTransceiver(MediaStreamTrackKind.Audio,
                new RTCRtpTransceiverInit { Direction = RTCRtpTransceiverDirection.SendOnly });

            var offer = await caller.CreateOffer();
            await caller.SetLocalDescription(offer);
            await callee.SetRemoteDescription(offer);

            // Seconds rather than the usual patience. A loopback track arrives in about a hundred
            // milliseconds where it arrives at all, and on the platforms where OnTrack never fires
            // it never will - so waiting the full thirty spent half a minute of every Apple run
            // learning nothing. Not a failure when it does not arrive either: that is the other
            // scenario's job to report, and saying it twice would make one defect look like two.
            await Task.WhenAny(tracked.Task, Task.Delay(TrackArrival));

            caller.Dispose();
            callee.Dispose();

            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            using var canary = Window().RTCPeerConnection(Configuration());
            canary.CreateDataChannel("canary");
            var probe = await canary.CreateOffer();

            return probe.Sdp is null
                ? "libwebrtc is still there but produced no SDP after a track was received"
                : null;
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
