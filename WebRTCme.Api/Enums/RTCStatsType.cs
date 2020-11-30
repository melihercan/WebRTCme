using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WebRTCme
{
    public enum RTCStatsType
    {
        [Description("candidate-pair")]
        CandidatePair,

        Certificate,
        Codec,
        Csrc,

        [Description("data-channel")]
        DataChannel,

        [Description("inbound-rtp")]
        InboundRtp,

        [Description("local-candidate")]
        LocalCandidate,

        [Description("outbound-rtp")]
        OutboundRtp,

        [Description("peer-connection")]
        PeerConnection,

        Receiver,

        [Description("remote-candidate")]
        RemoteCandidate,

        [Description("remote-inbound-rtp")]
        RemoteInboundRtp,

        [Description("remote-outbound-rtp")]
        RemoteOutboundRtp,

        Sender,
        Stream,
        Track,
        Transport
    }
}
