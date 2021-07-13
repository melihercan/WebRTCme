using System;
using WebRTCme.Connection.Signaling.Server.Enums;

namespace WebRTCme.Connection.Signaling.Server.TurnServerProxies
{
    public class TurnServerProxyFactory
    {
        readonly IServiceProvider _serviceProvider;

        public TurnServerProxyFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public ITurnServerProxy Create(TurnServer turnServer) =>
            turnServer switch
            {
                TurnServer.StunOnly => _serviceProvider.GetService(typeof(StunOnlyProxy)) as ITurnServerProxy,
                TurnServer.Xirsys => _serviceProvider.GetService(typeof(XirsysProxy)) as ITurnServerProxy,
                TurnServer.Coturn => _serviceProvider.GetService(typeof(CoturnProxy)) as ITurnServerProxy,
                TurnServer.AppRct => _serviceProvider.GetService(typeof(AppRtcProxy)) as ITurnServerProxy,
                TurnServer.Twilio => _serviceProvider.GetService(typeof(TwilioProxy)) as ITurnServerProxy,
                _ => throw new NotSupportedException($"'{turnServer}' is not supported")
            };
    }
}
