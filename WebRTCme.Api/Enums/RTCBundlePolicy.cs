using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RTCBundlePolicy
    {
        Balanced,
        
        [Description("max-compat")]
        MaxCompat,
        
        [Description("max-bundle")]
        MaxBundle,
    }
}
