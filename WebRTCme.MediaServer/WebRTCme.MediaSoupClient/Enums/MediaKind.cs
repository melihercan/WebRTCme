using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.MediaSoupClient.Enums
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum MediaKind
    {
        Audio,
        Video
    }
}
