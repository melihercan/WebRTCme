using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using WebRTCme;

namespace WebRtcMeMiddleware
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum RoomState
    {
        Idle,
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        Error
    }
}
