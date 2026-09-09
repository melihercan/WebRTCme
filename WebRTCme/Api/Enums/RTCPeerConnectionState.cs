using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum RTCPeerConnectionState
    {
        New,
        Connecting,
        Connected,
        Disconnected,
        Failed,
        Closed
    }
}
