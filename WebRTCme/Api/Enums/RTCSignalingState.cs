using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum RTCSignalingState
    {
        [EnumMember(Value = "stable")]
        Stable,

        [EnumMember(Value = "have-local-offer")]
        HaveLocalOffer,

        [EnumMember(Value = "have-remote-offer")]
        HaveRemoteOffer,

        [EnumMember(Value = "have-local-pranswer")]
        HaveLocalPranswer,

        [EnumMember(Value = "have-remote-pranswer")]
        HaveRemotePranswer,

        [EnumMember(Value = "closed")]
        Closed
    }
}
