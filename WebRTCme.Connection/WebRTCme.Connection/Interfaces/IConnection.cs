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
    }
}
