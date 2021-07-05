using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.MediaSoupClient.Enums
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum DtlsRole
    {
        Auto,
        Client,
        Server
    }
}
