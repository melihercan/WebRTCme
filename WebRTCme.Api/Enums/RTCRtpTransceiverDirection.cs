using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum RTCRtpTransceiverDirection
    {
        [EnumMember(Value = "sendrecv")]
        SendRecv,

        [EnumMember(Value = "sendonly")]
        SendOnly,

        [EnumMember(Value = "recvonly")]
        RecvOnly,

        [EnumMember(Value = "inactive")]
        Inactive
    }
}
