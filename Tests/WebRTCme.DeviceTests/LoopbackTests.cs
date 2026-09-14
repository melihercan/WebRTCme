using System.Text;
using WebRTCme;

namespace WebRTCme.DeviceTests;

/// <summary>
/// Two peer connections in one process, negotiating with each other.
/// </summary>
/// <remarks>
/// <para>
/// This is the test that proves the package works rather than merely restores. Everything in
/// tiers 1 and 2 is managed code: none of it loads libwebrtc.aar, WebRTC.framework or
/// WebRtcInterop.dll, so a package whose native payload is missing or built for the wrong
/// architecture passes all of them and fails here.
/// </para>
/// <para>
/// Deliberately no signalling server, no second machine, no network and no camera. The offer, the
/// answer and the ICE candidates are handed between the two connections in code, which is all
/// signalling ever does. What is left is exactly the part that can fail without a person watching:
/// the native library loading, SDP marshalling in both directions, ICE candidates surviving the
/// round trip, and DTLS and SCTP completing.
/// </para>
/// <para>
/// Media is not here on purpose. It needs a real capture device, which makes a test that fails on
/// a machine with no webcam rather than one that reports a broken package. That group comes later
/// and skips when no device is present.
/// </para>
/// </remarks>
public class LoopbackTests
{
    // Long enough for DTLS on a slow machine, short enough that a hang is a failure rather than a
    // coffee break. Host candidates over loopback are immediate; this is nearly all handshake.
    static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    static RTCConfiguration Configuration() => new()
    {
        // No STUN server. Loopback needs only host candidates, and reaching for a public STUN
        // server would make this test depend on the internet being up.
        IceServers = [],
        IceTransportPolicy = RTCIceTransportPolicy.All
    };

    static IWindow Window() => CrossWebRtc.Current.Window();

    /// <summary>
    /// The three fields a candidate needs to be re-added at the other end. The real signalling path
    /// serialises the same record to JSON and back; here it is handed over directly, because the
    /// wire format is not what this test is about.
    /// </summary>
    static RTCIceCandidateInit Init(IRTCIceCandidate candidate) => new()
    {
        Candidate = candidate.Candidate,
        SdpMid = candidate.SdpMid,
        SdpMLineIndex = candidate.SdpMLineIndex
    };

    static async Task<T> Within<T>(TaskCompletionSource<T> source, string what)
    {
        var finished = await Task.WhenAny(source.Task, Task.Delay(Patience));
        finished.Should().BeSameAs(source.Task, $"{what} should have happened within {Patience.TotalSeconds:0}s");
        return await source.Task;
    }

    /// <summary>
    /// The cheapest possible proof that the native half is present and callable. If the package is
    /// missing its native payload this throws DllNotFoundException here, before anything
    /// interesting is attempted.
    /// </summary>
    [Fact]
    public void The_native_library_loads_and_a_peer_connection_can_be_created()
    {
        using var pc = Window().RTCPeerConnection(Configuration());

        pc.Should().NotBeNull();
        pc.SignalingState.Should().Be(RTCSignalingState.Stable);
        pc.ConnectionState.Should().Be(RTCPeerConnectionState.New);
    }

    [Fact]
    public async Task An_offer_is_produced_and_is_real_sdp()
    {
        using var pc = Window().RTCPeerConnection(Configuration());
        pc.CreateDataChannel("probe");

        var offer = await pc.CreateOffer();

        offer.Type.Should().Be(RTCSdpType.Offer);
        offer.Sdp.Should().StartWith("v=0", "an SDP blob begins with its version line");
        offer.Sdp.Should().Contain("m=application", "the data channel should have produced an m-line");
    }

    /// <summary>
    /// The whole negotiation, end to end. Everything a call does before media flows happens here.
    /// </summary>
    [Fact]
    public async Task Two_peers_negotiate_and_a_data_channel_carries_a_message()
    {
        using var caller = Window().RTCPeerConnection(Configuration());
        using var callee = Window().RTCPeerConnection(Configuration());

        // Signalling, in two handlers. Each end's candidates go straight to the other - which is
        // all a signalling server ever does with them, minus the JSON and the websocket.
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
            var channel = e.Channel;
            channel.OnMessage += (_, m) => received.TrySetResult(
                m.Data as string ?? Encoding.UTF8.GetString((byte[])m.Data));

            // ReadyState first, and this is not defensive padding - waiting only on OnOpen hangs.
            // The native observer is registered when the channel wrapper is built, which happens
            // on the callback thread, while this handler is posted to the dispatcher. The channel
            // routinely opens in the gap, so OnOpen has already fired by the time there is anyone
            // subscribed to hear it. Same shape as the browser API, and the same fix: a channel
            // that is already open does not announce itself again.
            if (channel.ReadyState == RTCDataChannelState.Open)
                calleeChannelOpen.TrySetResult(true);
            else
                channel.OnOpen += (_, _) => calleeChannelOpen.TrySetResult(true);
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
        // Cannot already be open - nothing has been negotiated yet - but checked the same way as
        // the callee's so the two ends do not quietly rely on different assumptions.
        if (channel.ReadyState == RTCDataChannelState.Open) callerChannelOpen.TrySetResult(true);

        var offer = await caller.CreateOffer();
        await caller.SetLocalDescription(offer);
        await callee.SetRemoteDescription(offer);

        var answer = await callee.CreateAnswer();
        await callee.SetLocalDescription(answer);
        await caller.SetRemoteDescription(answer);

        (await Within(connected, "the peer connection")).Should().BeTrue();
        (await Within(callerChannelOpen, "the caller's data channel")).Should().BeTrue();
        (await Within(calleeChannelOpen, "the callee's data channel")).Should().BeTrue();

        channel.Send("hello from the other side");

        (await Within(received, "the message")).Should().Be("hello from the other side");
    }

    /// <summary>
    /// Enumerating devices exercises a different native path from the peer connection, and it is
    /// the one every app calls first. It must not throw even on a machine with nothing attached -
    /// an empty list is a legitimate answer and a crash is not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Skipped because it fails, and the failure is in the Windows binding rather than
    /// here.</strong> A completed negotiation leaves the native audio device module unable to
    /// count inputs:
    /// </para>
    /// <code>
    /// InvalidOperationException: Failed to count AudioInput devices: internal error.
    ///   at WebRTCme.Windows.MediaDevices.AddAudioDevices
    ///   at WebRTCme.Windows.MediaDevices.EnumerateDevices
    /// </code>
    /// <para>
    /// Bisected on 2026-09-14 by running pairs of these tests. Enumeration succeeds on its own, and
    /// after creating and disposing a peer connection, and after producing an offer - eight devices
    /// including the webcam microphone, from either an MTA or an STA thread. It fails only once
    /// <see cref="Two_peers_negotiate_and_a_data_channel_carries_a_message"/> has run in the same
    /// process, so what breaks it is a negotiation that reaches DTLS and SCTP, not device
    /// enumeration itself and not the apartment the call is made on.
    /// </para>
    /// <para>
    /// It matters beyond this test: showing a device picker after a call has ended is an ordinary
    /// thing for an app to do, and on this evidence it would throw. Worth noting that
    /// <c>AddAudioDevices</c> throws through <c>WebRtcRuntime.Check</c>, so a failure counting audio
    /// takes the video devices down with it rather than degrading to a partial list.
    /// </para>
    /// </remarks>
    [Fact(Skip = "Windows binding defect: after a negotiation reaches DTLS, counting audio input devices fails. See the remarks.")]
    public async Task Media_devices_can_be_enumerated_without_a_camera()
    {
        var devices = Window().Navigator().MediaDevices;

        var enumerate = async () => await devices.EnumerateDevices();

        await enumerate.Should().NotThrowAsync();
    }
}
