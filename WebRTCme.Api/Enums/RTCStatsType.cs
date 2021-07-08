using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum RTCStatsType
    {
        [EnumMember(Value = "candidate-pair")]
        CandidatePair,

        [EnumMember(Value = "certificate")]
        Certificate,

        [EnumMember(Value = "codec")]
        Codec,

        [EnumMember(Value = "csrc")]
        Csrc,

        [EnumMember(Value = "data-channel")]
        DataChannel,

        [EnumMember(Value = "inbound-rtp")]
        InboundRtp,

        [EnumMember(Value = "local-candidate")]
        LocalCandidate,

        [EnumMember(Value = "outbound-rtp")]
        OutboundRtp,

        [EnumMember(Value = "peer-connection")]
        PeerConnection,

        [EnumMember(Value = "receiver")]
        Receiver,

        [EnumMember(Value = "remote-candidate")]
        RemoteCandidate,

        [EnumMember(Value = "remote-inbound-rtp")]
        RemoteInboundRtp,

        [EnumMember(Value = "remote-outbound-rtp")]
        RemoteOutboundRtp,

        [EnumMember(Value = "sender")]
        Sender,

        [EnumMember(Value = "stream")]
        Stream,

        [EnumMember(Value = "track")]
        Track,

        [EnumMember(Value = "transport")]
        Transport
    }
}
