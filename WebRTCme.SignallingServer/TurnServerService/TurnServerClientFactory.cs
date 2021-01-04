using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class TurnServerClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TurnServerClientFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public ITurnServerClient Create(TurnServer turnServer) =>
            turnServer switch
            {
                TurnServer.Xirsys => _serviceProvider.GetService(typeof(XirsysClient)) as ITurnServerClient,
                TurnServer.Coturn => _serviceProvider.GetService(typeof(CoturnClient)) as ITurnServerClient,
                TurnServer.AppRct => _serviceProvider.GetService(typeof(AppRtcClient)) as ITurnServerClient,
                TurnServer.Twilio => _serviceProvider.GetService(typeof(TwilioClient)) as ITurnServerClient,
                _ => throw new NotSupportedException($"'{turnServer}' is not supported")
            };
    }
}
