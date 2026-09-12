using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public class PeerContext
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public IRTCPeerConnection PeerConnection { get; init; }
        public bool IsInitiator { get; init; }

        /// <summary>
        /// The sender carrying a shared screen to this peer, while one is being shared.
        /// </summary>
        /// <remarks>
        /// Per peer, because the sender belongs to the peer connection: a screen shared into a
        /// room of three is three senders on three transceivers. Held so that stopping can remove
        /// exactly the one that was added, rather than guessing which of a peer connection's video
        /// senders is the screen - which is the sort of guess that gets the camera removed instead.
        /// </remarks>
        public IRTCRtpSender ScreenSender { get; set; }

        /// <summary>
        /// The id of the track that sender is carrying, so the transceiver can be found again.
        /// </summary>
        /// <remarks>
        /// By id rather than by holding the transceiver, for the reason recorded against Blazor in
        /// the gaps document: <c>GetTransceivers</c> builds a new wrapper on every call, so an
        /// object kept from an earlier call never matches one from a later one. Track ids do.
        /// </remarks>
        public string ScreenTrackId { get; set; }

        /// <summary>
        /// The id of the stream this peer's camera and microphone arrive on.
        /// </summary>
        /// <remarks>
        /// Recorded from the first track this peer sends, and the only thing that distinguishes a
        /// second source from the first - see <c>SignalingConnection.OnTrack</c>. A peer-to-peer
        /// connection has no equivalent of mediasoup's <c>appData.source</c>: what the far side
        /// gets is an msid, and which msid means what is something it has to work out.
        /// </remarks>
        public string PrimaryStreamId { get; set; }
    }
}
