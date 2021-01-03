using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VideoType
    {
                // Source:
        Camera, // default, front, rear ...
        Room,   // Server.RoomId.UserId
        File,   // file name
        Url     // URL address
    }
}
