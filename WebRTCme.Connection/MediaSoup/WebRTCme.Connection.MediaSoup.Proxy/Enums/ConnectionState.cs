using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum ConnectionState
    {
        New,
        Connecting,
        Connected,
        Failed,
        Disconnected,
        Closed
    }
}
