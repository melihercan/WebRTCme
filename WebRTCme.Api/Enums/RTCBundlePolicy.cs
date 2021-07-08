using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum RTCBundlePolicy
    {
        [EnumMember(Value = "balanced")]
        Balanced,

        [EnumMember(Value = "max-compat")]
        MaxCompat,
        
        [EnumMember(Value = "max-bundle")]
        MaxBundle,
    }
}
