using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum TurnServer
    {
        StunOnly,
        Xirsys,
        Coturn,
        AppRct,
        Twilio
    }
}
