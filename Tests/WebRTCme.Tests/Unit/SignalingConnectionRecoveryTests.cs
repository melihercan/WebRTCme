using System.Reactive.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using NSubstitute;
using Utilme;
using WebRTCme.Connection;
using WebRTCme.Connection.Models;
using WebRTCme.Connection.Services;
using WebRTCme.Connection.Signaling;


namespace WebRTCme.Tests.Unit;

/// <summary>
/// What a call does when a peer's transport dies under it, and how it tears one down.
/// </summary>
/// <remarks>
/// <para>
/// A peer whose network path changes - a Wi-Fi roam, a DHCP renewal - loses its candidate pair and
/// never gets it back on its own. The transport goes to <c>Failed</c> and stays there while both
/// apps still look connected: the tile holds its last frame and nothing says otherwise. That is the
/// long-call decay in <c>doc/KnownGaps.md</c>, and it happened because the <c>Failed</c> branch here
/// was commented out with "WILL BE HANDLED BY PEER LEFT" - which never fires, because a peer whose
/// transport dies does not leave.
/// </para>
/// <para>
/// Pinned here rather than on a device because reproducing it for real means waiting half an hour
/// on three machines for a network event nobody controls. These are the rules the recovery follows,
/// and each one is a way the fix could go wrong in a manner nobody would notice until a call died
/// again.
/// </para>
/// </remarks>
public class SignalingConnectionRecoveryTests
{
    const int MaxIceRestartAttempts = 3;

    static readonly Guid PeerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    sealed class Harness
    {
        public SignalingConnection Connection { get; init; }
        public ISignalingServerApi Api { get; init; }
        public IRTCPeerConnection PeerConnection { get; init; }
        public List<PeerResponse> Responses { get; } = [];
        public IDisposable Subscription { get; set; }
        public IMediaStream LocalStream { get; init; }

        /// <summary>Offers sent so far: the first join plus one per recovery.</summary>
        public int OffersSent => Api.ReceivedCalls()
            .Count(call => call.GetMethodInfo().Name == nameof(ISignalingServerApi.SdpAsync));

        public IEnumerable<PeerResponse> Errors =>
            Responses.Where(r => r.Type == PeerResponseType.PeerError);

        public IEnumerable<PeerResponse> Of(PeerResponseType type) =>
            Responses.Where(r => r.Type == type);

        /// <summary>
        /// Drives a connection state change the way the bindings do, by raising the event and
        /// letting the handler read the state back off the peer connection.
        /// </summary>
        public void RaiseConnectionState(RTCPeerConnectionState state)
        {
            PeerConnection.ConnectionState.Returns(state);
            PeerConnection.OnConnectionStateChanged +=
                Raise.Event<EventHandler>(PeerConnection, EventArgs.Empty);
        }
    }

    /// <param name="joinGate">
    /// When given, the server's answer to the join is held back until the test completes it, and
    /// this returns as soon as the join has been sent - so a test can deliver what the server sends
    /// in between. Nothing further is set up: no peer has joined or offered.
    /// </param>
    static async Task<Harness> JoinedAsync(bool isInitiator = true,
        TaskCompletionSource<Result<Utilme.Unit>> joinGate = null)
    {
        var api = Substitute.For<ISignalingServerApi>();
        if (joinGate is null)
            api.JoinAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Result<Utilme.Unit>.Ok(Utilme.Unit.Value));
        else
            api.JoinAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(joinGate.Task);
        api.LeaveAsync(Arg.Any<Guid>()).Returns(Result<Utilme.Unit>.Ok(Utilme.Unit.Value));
        api.SdpAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns(Result<Utilme.Unit>.Ok(Utilme.Unit.Value));
        api.IceAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns(Result<Utilme.Unit>.Ok(Utilme.Unit.Value));
        api.GetIceServersAsync().Returns(Result<RTCIceServer[]>.Ok([]));

        var peerConnection = Substitute.For<IRTCPeerConnection>();
        peerConnection.CreateOffer().Returns(new RTCSessionDescriptionInit
        {
            Type = RTCSdpType.Offer,
            Sdp = "v=0"
        });
        peerConnection.CreateAnswer().Returns(new RTCSessionDescriptionInit
        {
            Type = RTCSdpType.Answer,
            Sdp = "v=0"
        });
        peerConnection.GetSenders().Returns([]);

        var window = Substitute.For<IWindow>();
        window.RTCPeerConnection(Arg.Any<RTCConfiguration>()).Returns(peerConnection);
        window.MediaStream().Returns(Substitute.For<IMediaStream>());

        var webRtc = Substitute.For<IWebRtc>();
        webRtc.Window(Arg.Any<IJSRuntime>()).Returns(window);

        // The answering side waits three quarters of a minute for the initiator on a real call.
        // A test that waited that out would be a test nobody runs.
        SignalingConnection.InitiatorRecoveryGrace = TimeSpan.FromMilliseconds(300);

        // A local stream with one audio track, because muting checks for one before it does
        // anything - and a test that stopped at that guard would never reach what it is about.
        var localTrack = Substitute.For<IMediaStreamTrack>();
        localTrack.Kind.Returns(MediaStreamTrackKind.Audio);
        var localStream = Substitute.For<IMediaStream>();
        localStream.GetAudioTracks().Returns([localTrack]);
        localStream.GetVideoTracks().Returns([]);

        var connection = new SignalingConnection(
            api, webRtc, NullLogger<SignalingConnection>.Instance);

        var harness = new Harness
        {
            Connection = connection,
            Api = api,
            PeerConnection = peerConnection,
            LocalStream = localStream
        };

        harness.Subscription = connection.ConnectionRequest(new UserContext
        {
            ConnectionType = ConnectionType.Signaling,
            Id = Guid.NewGuid(),
            Name = "tester",
            Room = "room",
            LocalStream = localStream
        }).Subscribe(harness.Responses.Add);

        // The subscription joins asynchronously; nothing below works until it has.
        await WaitUntilAsync(() =>
            api.ReceivedCalls().Any(c => c.GetMethodInfo().Name == nameof(ISignalingServerApi.JoinAsync)));

        if (joinGate is not null)
            return harness;

        if (isInitiator)
        {
            await connection.OnPeerJoinedAsync(PeerId, "peer");
        }
        else
        {
            // The answering side is created by an incoming offer, and only that path marks a peer
            // as not-initiator.
            await connection.OnPeerSdpAsync(PeerId, "peer",
                """{"type":"offer","sdp":"v=0"}""");
        }

        return harness;
    }

    /// <summary>
    /// Waits for work the recovery deliberately does off the caller's thread. Polls rather than
    /// sleeping a fixed time, so a passing run costs milliseconds and a broken one still fails
    /// rather than hanging.
    /// </summary>
    static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition() && DateTime.UtcNow < deadline)
            await Task.Delay(10);
    }

    /// <summary>
    /// An offer that arrives before this client's own join has returned is still answered.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The initiator offers the moment the server tells it this client has joined, and the server's
    /// reply to the join and the relayed offer come back on separate continuations. So the offer
    /// can be handled before the join's continuation has run - and that continuation is what created
    /// the call's context. The handler read a null context, threw, and reported the failure through
    /// that same null context, so the offer vanished without a trace: no answer, no error, no log.
    /// The joining side sat on the call page with only its own tile, forever.
    /// </para>
    /// <para>
    /// Found by a network soak on 2026-09-23 - Mac Catalyst joining and leaving a call with Windows
    /// over the real signalling server - in its ninth round. The server log showed the Mac join and
    /// Windows' offer and candidates relayed to it, and then nothing at all from the Mac, with every
    /// libwebrtc thread idle.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task AnOfferThatArrivesBeforeTheJoinReturnsIsStillAnswered()
    {
        var joinGate = new TaskCompletionSource<Result<Utilme.Unit>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var harness = await JoinedAsync(isInitiator: false, joinGate);

        // The offer first, as the server's receive loop delivers it; the join's reply after.
        var offerHandled = harness.Connection.OnPeerSdpAsync(PeerId, "peer",
            """{"type":"offer","sdp":"v=0"}""");
        joinGate.SetResult(Result<Utilme.Unit>.Ok(Utilme.Unit.Value));
        await offerHandled.WaitAsync(TimeSpan.FromSeconds(5));

        await WaitUntilAsync(() => harness.OffersSent > 0);

        harness.Api.Received(1).SdpAsync(PeerId, Arg.Is<string>(sdp => sdp.Contains("answer", StringComparison.OrdinalIgnoreCase)));
        harness.Errors.Should().BeEmpty("the offer belonged to this call and should simply be answered");
    }

    [Fact]
    public async Task AFailedTransportIsRestartedRatherThanLeftForDead()
    {
        var harness = await JoinedAsync();
        var offersAfterJoin = harness.OffersSent;

        harness.RaiseConnectionState(RTCPeerConnectionState.Failed);

        await WaitUntilAsync(() => harness.OffersSent > offersAfterJoin);

        harness.PeerConnection.Received(1).RestartIce();
        harness.OffersSent.Should().Be(offersAfterJoin + 1,
            "the restart only sets a flag - the offer that follows is what carries the new ICE " +
            "credentials to the peer");
    }

    [Fact]
    public async Task DisconnectedIsLeftAloneBecauseItOftenRecoversByItself()
    {
        var harness = await JoinedAsync();
        var offersAfterJoin = harness.OffersSent;

        harness.RaiseConnectionState(RTCPeerConnectionState.Disconnected);

        // Give a wrong implementation time to act, rather than passing because it was slow.
        await Task.Delay(200);

        harness.PeerConnection.DidNotReceive().RestartIce();
        harness.OffersSent.Should().Be(offersAfterJoin);
    }

    [Fact]
    public async Task OnlyTheInitiatorRestarts()
    {
        var harness = await JoinedAsync(isInitiator: false);
        var offersAfterJoin = harness.OffersSent;

        harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
        await Task.Delay(200);

        harness.PeerConnection.DidNotReceive().RestartIce();
        harness.OffersSent.Should().Be(offersAfterJoin,
            "both ends see Failed, and both offering is glare - two offers crossing, one of which " +
            "has to be rolled back - for no gain, since one restart re-runs the checks for the pair");
    }

    [Fact]
    public async Task RestartsAreBoundedSoAHopelessPeerIsNotAnOfferStorm()
    {
        var harness = await JoinedAsync();
        var offersAfterJoin = harness.OffersSent;

        for (var i = 0; i < MaxIceRestartAttempts + 2; i++)
        {
            harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
            await WaitUntilAsync(() => harness.OffersSent >= offersAfterJoin + Math.Min(i + 1, MaxIceRestartAttempts));
        }

        await Task.Delay(200);

        harness.PeerConnection.Received(MaxIceRestartAttempts).RestartIce();
        harness.OffersSent.Should().Be(offersAfterJoin + MaxIceRestartAttempts);
    }

    [Fact]
    public async Task ACallThatCannotBeRecoveredIsReportedRatherThanLeftLookingConnected()
    {
        var harness = await JoinedAsync();

        for (var i = 0; i < MaxIceRestartAttempts + 1; i++)
        {
            harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
            await Task.Delay(100);
        }

        await WaitUntilAsync(() => harness.Errors.Any());

        harness.Errors.Should().NotBeEmpty(
            "the whole defect was a dead call that still looked alive - saying nothing is the bug");
        harness.Errors.First().Id.Should().Be(PeerId);
    }

    /// <summary>
    /// The manual restart - the demo app's "restart ICE" button, and <c>IConnection.RestartIceAsync</c>
    /// - has to restart ICE on every platform, not just the one that reads the offer option.
    /// </summary>
    /// <remarks>
    /// It used to ask for <c>CreateOffer(new RTCOfferOptions { IceRestart = true })</c> and nothing
    /// else. Only Blazor honours that: it hands the options object to the browser's createOffer.
    /// Android passes <c>new MediaConstraints()</c>, iOS and Mac Catalyst pass
    /// <c>new RTCMediaConstraints(null, null)</c>, and Windows ignores the parameter. So on four of
    /// five platforms the button sent a plain re-offer, the call renegotiated, and nothing
    /// restarted - while the wiki said the feature was verified everywhere.
    ///
    /// Asserting on the offer option instead of this call would pass against exactly that bug,
    /// because the option was always being set. What matters is the call the bindings act on.
    /// </remarks>
    [Fact]
    public async Task TheManualRestartAsksThePlatformToRestartRatherThanJustSettingAnOption()
    {
        var harness = await JoinedAsync();
        var offersAfterJoin = harness.OffersSent;

        await harness.Connection.RestartIceAsync();

        harness.PeerConnection.Received(1).RestartIce();
        harness.OffersSent.Should().Be(offersAfterJoin + 1,
            "the restart sets a flag, and the offer after it is what carries the new credentials");
    }

    /// <summary>
    /// A recovery in progress has to be visible, because the tile cannot show it.
    /// </summary>
    /// <remarks>
    /// A peer whose transport died leaves its last frame on screen. It is a still picture of a
    /// person, which reads as a working call where nobody happens to be moving - so the failure
    /// mode of saying nothing is not "the user is uninformed", it is "the user believes the call
    /// is fine". Recovery takes seconds, and without this the only thing ever said is the error
    /// after the last attempt fails.
    /// </remarks>
    [Fact]
    public async Task ARecoveryInProgressIsAnnounced()
    {
        var harness = await JoinedAsync();

        harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
        await WaitUntilAsync(() => harness.Of(PeerResponseType.PeerReconnecting).Any());

        var announced = harness.Of(PeerResponseType.PeerReconnecting).ToList();
        announced.Should().ContainSingle("one attempt has been made, so one attempt is reported");
        announced[0].Id.Should().Be(PeerId);
        announced[0].Name.Should().Be("peer");
    }

    [Fact]
    public async Task ARecoveredPeerIsAnnouncedToo()
    {
        var harness = await JoinedAsync();

        harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
        await WaitUntilAsync(() => harness.Of(PeerResponseType.PeerReconnecting).Any());

        harness.RaiseConnectionState(RTCPeerConnectionState.Connected);
        await WaitUntilAsync(() => harness.Of(PeerResponseType.PeerReconnected).Any());

        harness.Of(PeerResponseType.PeerReconnected).Should().ContainSingle(
            "otherwise the tile stays covered by a Reconnecting overlay on a call that came back");
    }

    /// <summary>
    /// A peer connecting for the first time has not recovered from anything.
    /// </summary>
    /// <remarks>
    /// The obvious implementation raises PeerReconnected from the Connected branch unconditionally,
    /// which fires on every join and makes the signal meaningless.
    /// </remarks>
    [Fact]
    public async Task AFirstConnectionIsNotAnnouncedAsARecovery()
    {
        var harness = await JoinedAsync();

        harness.RaiseConnectionState(RTCPeerConnectionState.Connected);
        await Task.Delay(200);

        harness.Of(PeerResponseType.PeerReconnected).Should().BeEmpty();
    }

    /// <summary>
    /// A peer connection must never be closed on the thread that asked for the teardown.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On Apple, closing from the main thread deadlocks the process, and the chain is long enough
    /// that nothing about the crash points back here: <c>Close()</c> blocks on libwebrtc's
    /// signalling thread, which blocks on the worker, which reaches
    /// <c>VoiceProcessingAudioUnit::DisposeAudioUnit</c>, where Apple's
    /// <c>AudioComponentInstanceDispose</c> waits on a dispatch semaphore that needs the main run
    /// loop - the very thread waiting at the top. The app stops responding, the system kills it,
    /// and the report is <c>EXC_CRASH</c>/<c>SIGSEGV</c> with no faulting address. Diagnosed from
    /// five crash reports as WebRTCnative#5.
    /// </para>
    /// <para>
    /// The teardown is triggered from a thread of this test's own making rather than the test
    /// thread, which is what makes the assertion deterministic: <c>Task.Run</c> schedules onto the
    /// thread pool, and a dedicated thread is never a pool thread, so a close that happened inline
    /// cannot be mistaken for one that was moved.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task APeerConnectionIsNeverClosedOnTheCallersThread()
    {
        var harness = await JoinedAsync();

        var closedOn = 0;
        harness.PeerConnection.When(peer => peer.Close())
            .Do(_ => closedOn = Environment.CurrentManagedThreadId);

        var triggeredOn = 0;
        var trigger = new Thread(() =>
        {
            triggeredOn = Environment.CurrentManagedThreadId;
            harness.Subscription.Dispose();
        });
        trigger.Start();
        trigger.Join();

        await WaitUntilAsync(() => closedOn != 0);

        closedOn.Should().NotBe(0, "the peer connection should have been closed by the teardown");
        closedOn.Should().NotBe(triggeredOn,
            "closing on the thread that asked for the teardown deadlocks on Apple - the caller's " +
            "thread has to stay free to service the audio unit being disposed underneath it");
    }

    /// <summary>
    /// The answering side cannot restart, but it must not sit there silently either.
    /// </summary>
    /// <remarks>
    /// Recovery is the initiator's job - both ends restarting is glare. But an answerer that did
    /// nothing kept a tile frozen on its last frame with the call still looking connected, which is
    /// the very symptom the recovery exists to remove, surviving on the other side. Watched on
    /// hardware on 2026-09-22.
    /// </remarks>
    [Fact]
    public async Task TheAnswererSaysSoEvenThoughItCannotRestart()
    {
        var harness = await JoinedAsync(isInitiator: false);
        var offersAfterJoin = harness.OffersSent;

        harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
        await WaitUntilAsync(() => harness.Of(PeerResponseType.PeerReconnecting).Any());

        harness.Of(PeerResponseType.PeerReconnecting).Should().ContainSingle(
            "a frozen tile with nothing said is indistinguishable from a working call");

        harness.PeerConnection.DidNotReceive().RestartIce();
        harness.OffersSent.Should().Be(offersAfterJoin, "restarting from both ends is glare");
    }

    [Fact]
    public async Task TheAnswererReportsThePeerLostIfNobodyRecoversIt()
    {
        var harness = await JoinedAsync(isInitiator: false);

        harness.RaiseConnectionState(RTCPeerConnectionState.Failed);

        await WaitUntilAsync(() => harness.Errors.Any());

        harness.Errors.Should().NotBeEmpty(
            "waiting forever for an initiator that is never coming back is the old bug wearing a " +
            "different hat");
        harness.Errors.First().Id.Should().Be(PeerId);
    }

    /// <summary>
    /// And it stops saying so once the initiator's restart lands.
    /// </summary>
    /// <remarks>
    /// The obvious implementation raises PeerReconnected only where a restart was attempted, which
    /// is never true on this side - so the overlay would stay up over a call that had come back.
    /// </remarks>
    [Fact]
    public async Task TheAnswererClearsItselfWhenTheCallComesBack()
    {
        var harness = await JoinedAsync(isInitiator: false);

        harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
        await WaitUntilAsync(() => harness.Of(PeerResponseType.PeerReconnecting).Any());

        harness.RaiseConnectionState(RTCPeerConnectionState.Connected);
        await WaitUntilAsync(() => harness.Of(PeerResponseType.PeerReconnected).Any());

        harness.Of(PeerResponseType.PeerReconnected).Should().ContainSingle();
        harness.Errors.Should().BeEmpty("the call came back, so nothing was lost");
    }

    /// <summary>
    /// Leaving one call and joining another must not leave the second one contextless.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This connection is a DI singleton and a subscription's teardown is fire-and-forget, so the
    /// two overlap: a caller who hangs up and rejoins can have the second call running before the
    /// first has finished tearing down. The teardown used to clear the context unconditionally,
    /// which wiped the live call's - and then mute, screen share and statistics all failed with
    /// "there is no call", on a call that was plainly up and carrying video.
    /// </para>
    /// <para>
    /// Seen on Mac Catalyst on 2026-09-22, and made easier to hit by closing peer connections off
    /// the caller's thread - which is required there, so the race had to be closed rather than
    /// avoided.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task AFinishedCallDoesNotWipeTheOneThatReplacedIt()
    {
        var harness = await JoinedAsync();

        // A slow close, because the race needs the two to overlap and an instant teardown does not.
        first_close_is_slow(harness);

        // Both joins go through the SAME connection, which is what matters: it is registered as a
        // DI singleton, so one instance holds the context both calls write to. Subscribing a second
        // harness instead - which is what this test did at first - gives two objects with two
        // fields, and the race cannot happen.
        harness.Subscription.Dispose();

        var rejoined = harness.Connection.ConnectionRequest(new UserContext
        {
            ConnectionType = ConnectionType.Signaling,
            Id = Guid.NewGuid(),
            Name = "tester",
            Room = "room",
            LocalStream = harness.LocalStream
        }).Subscribe(_ => { });

        // Long enough for the first teardown to finish on top of the second join.
        await Task.Delay(900);

        var muting = async () => await harness.Connection.SetOutgoingMediaEnabledAsync(
            MediaStreamTrackKind.Audio, enabled: false);

        await muting.Should().NotThrowAsync<InvalidOperationException>(
            "the second call is live, so muting it must not report that there is no call");

        rejoined.Dispose();
    }

    /// <summary>Makes this harness's close slow, so a teardown outlives the join that follows it.</summary>
    static void first_close_is_slow(Harness harness) =>
        harness.PeerConnection.When(peer => peer.Close()).Do(_ => Thread.Sleep(400));

    [Fact]
    public async Task ConnectingAgainRestoresTheAllowance()
    {
        var harness = await JoinedAsync();

        // Spend the allowance.
        for (var i = 0; i < MaxIceRestartAttempts + 1; i++)
        {
            harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
            await Task.Delay(100);
        }
        harness.PeerConnection.Received(MaxIceRestartAttempts).RestartIce();

        // A call that comes back and later fails again is a new problem, not a continuation.
        harness.RaiseConnectionState(RTCPeerConnectionState.Connected);
        await Task.Delay(50);

        harness.PeerConnection.ClearReceivedCalls();
        harness.RaiseConnectionState(RTCPeerConnectionState.Failed);
        await WaitUntilAsync(() => harness.PeerConnection.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == nameof(IRTCPeerConnection.RestartIce)));

        harness.PeerConnection.Received(1).RestartIce();
    }
}
