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

        ProducerDataChannel,
        ConsumerDataChannel,
    }
}
