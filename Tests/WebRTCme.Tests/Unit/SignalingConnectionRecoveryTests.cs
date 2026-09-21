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
/// What a call does when a peer's transport dies under it.
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

    static async Task<Harness> JoinedAsync(bool isInitiator = true)
    {
        var api = Substitute.For<ISignalingServerApi>();
        api.JoinAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Result<Utilme.Unit>.Ok(Utilme.Unit.Value));
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

        var connection = new SignalingConnection(
            api, webRtc, NullLogger<SignalingConnection>.Instance);

        var harness = new Harness
        {
            Connection = connection,
            Api = api,
            PeerConnection = peerConnection
        };

        harness.Subscription = connection.ConnectionRequest(new UserContext
        {
            ConnectionType = ConnectionType.Signaling,
            Id = Guid.NewGuid(),
            Name = "tester",
            Room = "room"
        }).Subscribe(harness.Responses.Add);

        // The subscription joins asynchronously; nothing below works until it has.
        await WaitUntilAsync(() =>
            api.ReceivedCalls().Any(c => c.GetMethodInfo().Name == nameof(ISignalingServerApi.JoinAsync)));

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
