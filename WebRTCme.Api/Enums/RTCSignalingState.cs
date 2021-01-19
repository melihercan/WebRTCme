using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum RTCSignalingState
    {
        Stable,

        [Description("have-local-offer")]
        HaveLocalOffer,

        [Description("have-remote-offer")]
        HaveRemoteOffer,

        [Description("have-local-pranswer")]
        HaveLocalPranswer,

        [Description("have-remote-pranswer")]
        HaveRemotePranswer
    }
}
