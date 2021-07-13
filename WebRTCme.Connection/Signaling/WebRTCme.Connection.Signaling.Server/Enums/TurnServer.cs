using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.Signaling.Server.Enums
{
    public enum TurnServer
    {
        StunOnly,
        Xirsys,
        Coturn,
        AppRct,
        Twilio
    }
}
