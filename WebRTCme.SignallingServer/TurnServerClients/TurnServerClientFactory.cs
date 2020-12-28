using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerClients
{
    internal static class TurnServerClientFactory
    {
        internal static ITurnServerClient Create(TurnServer turnServer) =>
            turnServer switch
            {
                TurnServer.Xirsys => new XirsysClient(),
                TurnServer.Coturn => new CoturnClient(),
                TurnServer.AppRct => new AppRtcClient(),
                TurnServer.Twilio => new TwilioClient(),
                _ => throw new NotSupportedException($"'{turnServer}' is not supported")
            };
    }
}
