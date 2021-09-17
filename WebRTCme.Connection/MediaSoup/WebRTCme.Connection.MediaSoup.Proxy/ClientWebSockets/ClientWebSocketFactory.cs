using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.ClientWebSockets
{
    public class ClientWebSocketFactory
    {
        readonly IServiceProvider _serviceProvider;

        public ClientWebSocketFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public IClientWebSocket Create(ClientWebSocketSelect select) =>
            select switch
            {
                ClientWebSocketSelect.System => 
                    _serviceProvider.GetService(typeof(ClientWebSocketSystem)) as IClientWebSocket,
                ClientWebSocketSelect.LitePcl =>
                    _serviceProvider.GetService(typeof(ClientWebSocketLitePcl)) as IClientWebSocket,
                _ => throw new NotSupportedException($"'{select}' is not supported")
            };
    }
}
