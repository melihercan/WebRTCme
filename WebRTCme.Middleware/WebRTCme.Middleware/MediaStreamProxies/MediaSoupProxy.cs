using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;

namespace WebRTCme.Middleware.MediaStreamProxies
{
    class MediaSoupProxy : IMediaServerProxy
    {
        readonly ClientWebSocket _webSocket = new();

        string _mediaSoupServerBaseUrl;

        public MediaSoupProxy(IConfiguration configuration)
        {
            _mediaSoupServerBaseUrl = configuration["MediaSoupServer:BaseUrl"];
        }

        public async Task InitAsync()
        {
//            await _webSocket.ConnectAsync(new Uri(_mediaSoupServerBaseUrl))
        }

        public async Task JoinAsync(ConnectionRequestParameters connectionRequestParameters)
        {
            try
            {

                //'wss://192.168.1.48:4443/?roomId=qt95tmpv&peerId=g0iywgxd'
                // "protoo" 
                // "Sec-WebSocket-Protocol"
                CancellationTokenSource cts = new();
                var uri = new Uri(new Uri(_mediaSoupServerBaseUrl), 
                    $"?roomId={connectionRequestParameters.ConnectionParameters.RoomName}" +
                    $"&peerId={connectionRequestParameters.ConnectionParameters.UserName}");
                _webSocket.Options.AddSubProtocol("protoo");
                _webSocket.Options.AddSubProtocol("Sec-WebSocket-Protocol");
                await _webSocket.ConnectAsync(uri, cts.Token);
            }
            catch(Exception ex)
            {
                var m = ex.Message;
            }

        }


    }
}
