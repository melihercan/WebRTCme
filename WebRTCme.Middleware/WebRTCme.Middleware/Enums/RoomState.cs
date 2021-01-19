using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRtcMeMiddleware
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
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
