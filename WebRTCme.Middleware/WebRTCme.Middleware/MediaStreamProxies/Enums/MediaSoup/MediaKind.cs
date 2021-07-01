using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Middleware.MediaStreamProxies.Enums.MediaSoup
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    enum MediaKind
    {
        Audio,
        Video
    }
}
