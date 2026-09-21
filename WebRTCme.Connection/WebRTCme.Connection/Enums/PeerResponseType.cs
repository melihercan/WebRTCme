using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public enum PeerResponseType
    {
        PeerJoined,
        PeerLeft,
        PeerMedia,
        PeerError,

        /// <summary>
        /// This peer's transport failed and is being restarted. Raised once per attempt.
        /// </summary>
        /// <remarks>
        /// Recovery takes seconds, and until it finishes the tile holds its last frame - which
        /// reads as a working call that has gone quiet. Without this the only thing a user ever
        /// sees is the <see cref="PeerError"/> after the last attempt fails, and nothing at all
        /// when recovery succeeds.
        /// </remarks>
        PeerReconnecting,

        /// <summary>
        /// A peer that was being restarted is connected again.
        /// </summary>
        PeerReconnected,

        ProducerDataChannel,
        ConsumerDataChannel,
    }
}
