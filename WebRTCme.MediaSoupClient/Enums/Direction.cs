using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.MediaSoupClient.Enums
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    enum Direction
    {
        Sendrecv,
        Sendonly,
        Recvonly
    }
}
