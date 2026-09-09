using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Middleware
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum DataFromType
    {
        System,
        Incoming,
        Outgoing
    }
}
