using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text.Json;
using WebRTCme.Middleware.MediaStreamProxies.Models;
using WebRTCme.Middleware.MediaStreamProxies.Enums;
using WebRTCme.Middleware.MediaStreamProxies.Enums.MediaSoup;
using WebRTCme.Middleware.MediaStreamProxies.Models.MediaSoup;

namespace WebRTCme.Middleware.MediaStreamProxies
{
    class MediaSoupProxy : IMediaServerProxy
    {
        readonly ClientWebSocket _webSocket = new();
        readonly ArraySegment<byte> _rxBuffer = new(new byte[16384]);


        TaskCompletionSource<ProtooResponseOk> _tcs;
        CancellationTokenSource _cts;
        string _mediaSoupServerBaseUrl;
        static uint _counter;
        SemaphoreSlim _sem = new(1);

        public MediaSoupProxy(IConfiguration configuration)
        {
            _mediaSoupServerBaseUrl = configuration["MediaSoupServer:BaseUrl"];
        }


        public async Task StartAsync(ConnectionRequestParameters connectionRequestParameters)
        {
            _cts = new();

            var uri = new Uri(new Uri(_mediaSoupServerBaseUrl),
                $"?roomId={connectionRequestParameters.ConnectionParameters.RoomName}" +
                $"&peerId={connectionRequestParameters.ConnectionParameters.UserName}");
            _webSocket.Options.AddSubProtocol("protoo");
            _webSocket.Options.AddSubProtocol("Sec-WebSocket-Protocol");
            await _webSocket.ConnectAsync(uri, _cts.Token);

            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var bytes = await _webSocket.ReceiveAsync(_rxBuffer, _cts.Token);
                        var json = Encoding.UTF8.GetString(_rxBuffer.Array, 0, bytes.Count);
                        var jsonDocument = JsonDocument.Parse(json);
                        if (jsonDocument.RootElement.TryGetProperty("response", out var r))
                        {
                            var ok = jsonDocument.RootElement.GetProperty("ok").GetBoolean();
                            if (ok)
                            {
                                var responseOk = JsonSerializer.Deserialize<ProtooResponseOk>(json,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                _tcs?.SetResult(responseOk);
                            }
                            else
                            {
                                var responseError = JsonSerializer.Deserialize<ProtooResponseError>(json,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                _tcs.SetException(new Exception($"{responseError.ErrorReason}"));
                            }
                        }
                        else if (jsonDocument.RootElement.TryGetProperty("notification", out var n))
                        {
                            var notification = JsonSerializer.Deserialize<ProtooNotification>(json,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);

                            //// TODO: ANALYZE AND INVOKE EVENT
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            });
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            _cts.Dispose();
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
        }

        public async Task JoinAsync()
        {
            var routerRtpCapabilities = await ProtooTransactionAsync(MethodName.GetRouterRtpCapabilities);
            _ = await ProtooTransactionAsync(MethodName.CreateWebRtcTransport, new WebRtcTransportCreateParameters 
            {
                ForceTcp = false,
                Producing = true,
                Consuming = false,
                SctpCapabilities = new SctpCapabilities 
                { 
                    NumStream = new NumSctpStreams 
                    { 
                        Os = 1024,
                        Mis = 1024,
                    }
                }
            });
        }

        public Task LeaveAsync()
        {
            throw new NotImplementedException();
        }

        async Task<object> ProtooTransactionAsync(string method, object data = null)
        {
            try
            {
                await _sem.WaitAsync();
                _tcs = new();

                var request = new ProtooRequest
                {
                    Request = true,
                    Id = _counter++,
                    Method = method,
                    Data = data
                };

                await _webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize(request, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions))),
                    WebSocketMessageType.Text,
                    true,
                    _cts.Token);

                var response = await _tcs.Task;
                if (response.Id != request.Id)
                    throw new Exception($"request.Id:{request.Id} and response.Id:{response.Id} are different!");

                /// TODO: CONVERT DATA TO MODEL
                /// 
                switch (method)
                {
                    case MethodName.GetRouterRtpCapabilities:
                        var json = ((JsonElement)response.Data).GetRawText();
                        var routerRtpCapabilities = JsonSerializer.Deserialize<RouterRtpCapabilities>(
                            json, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                        foreach(var codec in routerRtpCapabilities.Codecs)
                        {
                            var parametersJson = ((JsonElement)codec.Parameters).GetRawText();
                            if (codec.MimeType.Equals("audio/opus"))
                            {
                                var opus = JsonSerializer.Deserialize<OpusParameters>(parametersJson,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                codec.Parameters = opus;
                            }
                            if (codec.MimeType.Equals("video/H264"))
                            {
                                var h264 = JsonSerializer.Deserialize<H264Parameters>(parametersJson,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                codec.Parameters = h264;
                            }
                            else if (codec.MimeType.Equals("video/VP8"))
                            {
                                var vp8 = JsonSerializer.Deserialize<VP8Parameters>(parametersJson,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                codec.Parameters = vp8;
                            }
                            else if (codec.MimeType.Equals("video/VP9"))
                            {
                                var vp9 = JsonSerializer.Deserialize<VP9Parameters>(parametersJson,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                codec.Parameters = vp9;
                            }
                            else if (codec.MimeType.Equals("video/rtx"))
                            {
                                var rtx = JsonSerializer.Deserialize<RtxParameters>(parametersJson,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                codec.Parameters = rtx;
                            }
                            else
                                codec.Parameters = null;
                        }
                        return routerRtpCapabilities;
                }
                return null;
            }
            finally
            {
                _tcs.Task.Dispose();
                _tcs = null;
                _sem.Release();
            }
        }
    }
}
