using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text.Json;
using WebRTCme.Middleware.MediaStreamProxies.Models;

namespace WebRTCme.Middleware.MediaStreamProxies
{
    class MediaSoupProxy : IMediaServerProxy
    {
        readonly ClientWebSocket _webSocket = new();

        string _mediaSoupServerBaseUrl;
        static uint _counter;

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


                //var pr = new ProtooRequest
                //{
                //    Id = _counter++,
                //    Method = "getRouterRtpCapabilities",
                //};

                //var json = JsonSerializer.Serialize(pr, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);


                await _webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize(new ProtooRequest 
                        { 
                            Request = true,
                            Id = _counter++,
                            Method = "getRouterRtpCapabilities",
                        }, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions))),
                    WebSocketMessageType.Text,
                    true,
                    cts.Token);

                var buffer = new ArraySegment<byte>(new byte[1024]); 
                var received = await _webSocket.ReceiveAsync(buffer, cts.Token);
                var receivedAsText = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);




            }
            catch(Exception ex)
            {
                var m = ex.Message;
            }

        }


    }
}
