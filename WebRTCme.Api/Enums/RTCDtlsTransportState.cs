using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum RTCDtlsTransportState
    {
        New,
        Connecting,
        Connected,
        Closed,
        Failed
    }
}
