using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Connection
{
    public interface IConnection
    {

        IObservable<PeerResponse> ConnectionRequest(UserContext userContext);

        Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack);

        Task<IRTCStatsReport> GetStats(Guid id);

        /// <summary>
        /// Statistics for what this client is sending.
        /// </summary>
        /// <remarks>
        /// <see cref="GetStats(Guid)"/> answers a question about one peer, which on the mediasoup
        /// path can only ever be a receive-side answer: the peers arrive as consumers, and the
        /// producers that carry this client's own media belong to no peer in particular. So the
        /// outbound bitrate, the encoder's quality limitation and the loss the far side reports
        /// back were all unreachable. This asks the senders directly instead.
        ///
        /// Peer-to-peer has a send side per peer, since each peer connection encodes separately,
        /// and all of them are reported here at once. Their keys are qualified with the peer's id
        /// to keep two peers' streams apart, so read the entries by <see cref="RTCStats.Type"/>
        /// rather than by key.
        ///
        /// Empty rather than throwing when nothing is being sent yet - producing starts
        /// asynchronously, so a poller would otherwise have to race it.
        /// </remarks>
        Task<IRTCStatsReport> GetOutgoingStatsAsync();

        /// <summary>
        /// Whether this client is currently sending <paramref name="kind"/>.
        /// </summary>
        /// <remarks>
        /// The connection holds this rather than the caller, because the caller is not the only
        /// thing that can change it: on the mediasoup path the producer's own paused flag is
        /// where the truth lives, and a reconnect resets it without anyone asking.
        /// </remarks>
        bool IsOutgoingMediaEnabled(MediaStreamTrackKind kind);

        /// <summary>
        /// Mutes or unmutes what this client sends, and tells the other peers.
        /// </summary>
        /// <remarks>
        /// Mute is not "stop the track": stopping releases the camera and needs renegotiation to
        /// undo. Both paths leave the sender in place and stop what flows through it - the
        /// peer-to-peer path by disabling the local track, so peers receive silence and black
        /// frames, and the mediasoup path by pausing the producer on the server as well, so
        /// nothing is forwarded at all.
        ///
        /// Peers are told either way. Peer-to-peer that is an explicit signalling message, which
        /// arrives as <see cref="PeerResponseType.PeerMedia"/>; on the mediasoup path the server
        /// pauses the matching consumers and each peer hears it from its own notification.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// This connection is not sending <paramref name="kind"/> - either there is no call, or no
        /// track of that kind was offered when it started.
        /// </exception>
        Task SetOutgoingMediaEnabledAsync(MediaStreamTrackKind kind, bool enabled);

        /// <summary>
        /// Re-gathers ICE, recovering a call whose network path has died under it.
        /// </summary>
        /// <remarks>
        /// For when the media stops but the signalling is still alive - a device changing network,
        /// a NAT binding expiring, a route disappearing. Without it such a call stays dead until
        /// somebody hangs up and redials, which is what happened here until now: both paths could
        /// restart ICE and neither had any way to be asked.
        ///
        /// The two paths do different work. Peer-to-peer renegotiates with each peer, offering
        /// afresh with the ICE-restart flag set; mediasoup asks the server to re-gather on both
        /// transports and applies the parameters it sends back.
        /// </remarks>
        /// <exception cref="InvalidOperationException">There is no call to restart.</exception>
        Task RestartIceAsync();

        /// <summary>
        /// Asks for the best simulcast layers to forward from one peer.
        /// </summary>
        /// <remarks>
        /// A <b>ceiling, not a floor</b>. The server clamps the request to the layers the producer
        /// actually publishes, and still drops below it whenever it decides it has to - so asking
        /// for the top layer does not make the top layer arrive. This is the reason
        /// <c>setConsumerPreferredLayers</c> was no help against "The SFU will not climb past the
        /// bottom layer" in the gaps document: it cannot raise a floor that the server has already
        /// put down.
        ///
        /// What it is genuinely for is the other direction - capping what a peer costs. A tile
        /// shown as a thumbnail has no use for the top layer, and saying so saves the downlink.
        ///
        /// Layers are indices from 0, counting up. Audio consumers have none and are skipped.
        /// </remarks>
        /// <exception cref="ArgumentException">No such peer is in this connection.</exception>
        /// <exception cref="NotSupportedException">
        /// This connection carries no simulcast, so there are no layers to choose between.
        /// </exception>
        Task SetPreferredIncomingLayersAsync(Guid peerId, int spatialLayer, int temporalLayer);

        /// <summary>
        /// Caps the simulcast layers this client encodes and sends.
        /// </summary>
        /// <remarks>
        /// Every layer up to and including <paramref name="spatialLayer"/> stays active and the
        /// rest are switched off, so this is a real limit rather than a preference: the frames are
        /// never encoded, and the CPU and uplink are not spent. Useful on battery, on a metered
        /// connection, or in a call where nobody is displaying this client large.
        ///
        /// Layer 0 alone is not the same as muting - it keeps a picture flowing at the smallest
        /// size, where <see cref="SetOutgoingMediaEnabledAsync"/> stops it.
        /// </remarks>
        /// <exception cref="InvalidOperationException">This connection is not sending video.</exception>
        /// <exception cref="NotSupportedException">
        /// This connection carries no simulcast, so there are no layers to cap.
        /// </exception>
        Task SetMaxOutgoingSpatialLayerAsync(int spatialLayer);
    }
}
