using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Middleware
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum VideoType
    {
                // Source:
        Camera, // default, front, rear ...
        Room,   // Server.RoomId.UserId
        File,   // file name
        Url     // URL address
    }
}
