using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RTCDtxStatus
    {
        Disabled,
        Enabled
    }
}
