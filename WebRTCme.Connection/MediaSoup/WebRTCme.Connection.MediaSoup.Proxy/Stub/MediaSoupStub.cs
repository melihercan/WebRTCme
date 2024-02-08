using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Utilme;
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.ClientWebSockets;
////using Xamarin.Essentials;
using Microsoft.Maui.Devices;

namespace WebRTCme.Connection.MediaSoup.Proxy.Stub
{
    class MediaSoupStub : IMediaSoupServerApi
    {
        readonly IClientWebSocket _webSocket;
        readonly ArraySegment<byte> _rxBuffer = new(new byte[16384]);

        TaskCompletionSource<ProtooResponseOk> _tcsResponseOk;
        TaskCompletionSource<ProtooRequest> _tcsRequest;
        TaskCompletionSource<ProtooNotification> _tcsNotification;
        CancellationTokenSource _cts;

        Channel<ProtooResponse> _responseChannel = Channel.CreateBounded<ProtooResponse>(10);
        //// TODO: It seems media soup server is sending all available consumers and data consumers in a stream
        /// of requests one after anohter. To avoid message overflow consider using unbounded channel!!!
        Channel<ProtooRequest> _requestChannel = Channel.CreateBounded<ProtooRequest>(50);
        Channel<ProtooNotification> _notificationChannel = Channel.CreateBounded<ProtooNotification>(10);
        Dictionary<uint, TaskCompletionSource<ProtooResponse>> _apiRequests = new();


        string _mediaSoupServerBaseUrl;
        static uint _counter;
        ////SemaphoreSlim _sem = new(1);

        public event IMediaSoupServerNotify.NotifyDelegateAsync NotifyEventAsync;
        public event IMediaSoupServerNotify.RequestDelegateAsync RequestEventAsync;

        public MediaSoupStub(ClientWebSocketFactory clientWebSocketFactory, IConfiguration configuration, 
            IWebRtc webRtc, ILogger<MediaSoupStub> logger, IJSRuntime jsRuntime = null)
        {
            _mediaSoupServerBaseUrl = configuration["MediaSoupServer:BaseUrl"];
            Registry.WebRtc = webRtc;
            Registry.Logger = logger;
            Registry.JsRuntime = jsRuntime;

            //// TODO: Bypass only for debugging with self signed certs (local IPs).
            var bypassSslCertificateError = DeviceInfo.Platform == DevicePlatform.Android;
            if (bypassSslCertificateError)
            {
                _webSocket = clientWebSocketFactory.Create(ClientWebSocketSelect.LitePcl);
                _webSocket.Options.IgnoreServerCertificateErrors = true;
            }
            else
                _webSocket = clientWebSocketFactory.Create(ClientWebSocketSelect.System);
        }

        public async Task<Result<Unit>> ConnectAsync(Guid id, string name, string room)
        {
            _cts = new();


            // This throws in Blazor!!!
#if false
            var uri = new Uri(_mediaSoupServerBaseUrl);
            _webSocket.Options.SetRequestHeader("roomId", room);
            _webSocket.Options.SetRequestHeader("peerId", name);
#endif
            var uri = new Uri(new Uri(_mediaSoupServerBaseUrl),
                $"?roomId={room}" +
                $"&peerId={name}");
            _webSocket.Options.AddSubProtocol("protoo");
            _webSocket.Options.AddSubProtocol("Sec-WebSocket-Protocol");

            try
            {
                await _webSocket.ConnectAsync(uri, _cts.Token);
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }

            // Task handling incoming requests.
            _ = Task.Run(async () => 
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        ////_tcsRequest = new();
                        ////var request = await _tcsRequest.Task;
                        var request = await _requestChannel.Reader.ReadAsync(_cts.Token);
Console.WriteLine($"########################## REQUEST: {request.Method}");

                        await RequestEventAsync?.Invoke(request.Method, request.Data,
                            // accept
                            async (data) =>
                            {
                                try
                                {
                                    ////await _sem.WaitAsync();
                                    var response = new ProtooResponse
                                    {
                                        Response = true,
                                        Id = request.Id,
                                        Ok = true,
                                        Data = data
                                    };
                                    var json = JsonSerializer.Serialize(response,
                                        JsonHelper.WebRtcJsonSerializerOptions);
          Console.WriteLine($"<<<<<<<<<<<<< OUTGOING MSG (REQUEST ACCEPT): {json}");
                                    await _webSocket.SendAsync(
                                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)),
                                        WebSocketMessageType.Text,
                                        true,
                                        _cts.Token);
                                    Registry.Logger.LogInformation($"<======= OnRequestAsync Response: {request.Method}");
                                }
                                finally
                                {
                                    ////_sem.Release();
                                }
                            },
                            // reject
                            async (error, errorReason) =>
                            {
                                try
                                {
                                    ////await _sem.WaitAsync();
                                    var response = new ProtooResponse
                                    {
                                        Response = true,
                                        Id = request.Id,
                                        Ok = false,
                                        ErrorCode = error,
                                        ErrorReason = errorReason
                                    };
                                    var json = JsonSerializer.Serialize(response,
                                        JsonHelper.WebRtcJsonSerializerOptions);
         Console.WriteLine($"<<<<<<<<<<<<< OUTGOING MSG (REQUEST ERROR): {json}");
                                    await _webSocket.SendAsync(
                                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)),
                                        WebSocketMessageType.Text,
                                        true,
                                        _cts.Token);
                                    Registry.Logger.LogInformation($"<======= OnRequestAsync Error: {request.Method}");
                                }
                                finally
                                {
                                    ////_sem.Release();
                                }
                            });

                    }
                    catch (Exception ex)
                    {
                        Registry.Logger.LogError($"E X C E P T I O N: {ex.Message}");
                    }
                    finally
                    {
                    }
                }
            });

            // Task handling incoming notifications.
            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        ////_tcsNotification = new();
                        ////var notification = await _tcsNotification.Task;
                        var notification = await _notificationChannel.Reader.ReadAsync(_cts.Token);

                        await NotifyEventAsync?.Invoke(notification.Method, notification.Data);
                    }
                    catch (Exception ex)
                    {
                        Registry.Logger.LogError($"E X C E P T I O N: {ex.Message}");
                    }
                    finally
                    {

                    }
                }
            });

            // Task handling responses.
            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var response = await _responseChannel.Reader.ReadAsync(_cts.Token);
                        var tcs = _apiRequests[response.Id];
                        tcs.SetResult(response);
                        _apiRequests.Remove(response.Id);
                    }
                    catch (Exception ex)
                    {
                        Registry.Logger.LogError($"E X C E P T I O N: {ex.Message}");
                    }
                    finally
                    {

                    }
                }
            });

            // Task handling incoming messages and dispatching them.
            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _webSocket.ReceiveAsync(_rxBuffer, _cts.Token);
                        var json = Encoding.UTF8.GetString(_rxBuffer.Array, 0, result.Count);
  Console.WriteLine($">>>>>>>>>>>>> INCOMING MSG: {json}");
                        var jsonDocument = JsonDocument.Parse(json);
                        if (jsonDocument.RootElement.TryGetProperty("response", out _))
                        {
                            var ok = jsonDocument.RootElement.TryGetProperty("ok", out _);
                            if (ok)
                            {
                                var responseOk = JsonSerializer.Deserialize<ProtooResponseOk>(json,
                                    JsonHelper.WebRtcJsonSerializerOptions);
                                ////_tcsResponseOk?.SetResult(responseOk);
                                await _responseChannel.Writer.WriteAsync(new ProtooResponse 
                                { 
                                    Response = responseOk.Response,
                                    Id = responseOk.Id,
                                    Ok = responseOk.Ok,
                                    Data = responseOk.Data
                                });
                            }
                            else
                            {
                                var responseError = JsonSerializer.Deserialize<ProtooResponseError>(json,
                                    JsonHelper.WebRtcJsonSerializerOptions);
                                Registry.Logger.LogError(responseError.ErrorReason);
                                ////_tcsResponseOk?.SetException(new Exception($"{responseError.ErrorReason}"));
                                await _responseChannel.Writer.WriteAsync(new ProtooResponse
                                {
                                    Response = responseError.Response,
                                    Id = responseError.Id,
                                    Ok = responseError.Ok,
                                    ErrorCode = responseError.ErrorCode,
                                    ErrorReason = responseError.ErrorReason
                                });
                            }
                        }
                        else if (jsonDocument.RootElement.TryGetProperty("request", out _))
                        {
                            var request = JsonSerializer.Deserialize<ProtooRequest>(json,
                                JsonHelper.WebRtcJsonSerializerOptions);
                            ////_tcsRequest?.SetResult(request);
                            await _requestChannel.Writer.WriteAsync(request);
                        }
                        else if (jsonDocument.RootElement.TryGetProperty("notification", out _))
                        {
                            var notification = JsonSerializer.Deserialize<ProtooNotification>(json,
                                    JsonHelper.WebRtcJsonSerializerOptions);
                            ////_tcsNotification?.SetResult(notification);
                            await _notificationChannel.Writer.WriteAsync(notification);
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Registry.Logger.LogError($"E X C E P T I O N: {ex.Message}");
                        //// TODO: HOW TO REPORT THIS ERROR??? ERROR EVENT???
                    }
                }
            });

            return Result<Unit>.Ok(Unit.Default);
        }


        public async Task<Result<object>> ApiAsync(string method, object data)
        {
            Registry.Logger.LogInformation($"######## CallAsync: {method}");
            try
            {
                ////await _sem.WaitAsync();
                ////_tcsResponseOk = new();

                var request = new ProtooRequest
                {
                    Request = true,
                    Id = _counter++,
                    Method = method,
                    Data = data
                };
                var json = JsonSerializer.Serialize(request, JsonHelper.WebRtcJsonSerializerOptions);

                TaskCompletionSource<ProtooResponse> tcs = new();
                _apiRequests.Add(request.Id, tcs);

  Console.WriteLine($"<<<<<<<<<<<<< OUTGOING MSG (CALL): {json}");
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)),
                    WebSocketMessageType.Text,
                    true,
                    _cts.Token);

                ////var response = await _tcsResponseOk.Task;
                var response = await tcs.Task;
                if (response.Id != request.Id)
                    throw new Exception($"request.Id:{request.Id} and response.Id:{response.Id} are different!");
                if (!response.Ok)
                    throw new Exception(response.ErrorReason);

                return Result<object>.Ok(response.Data);
            }
            catch (Exception ex)
            {
                return Result<object>.Error(ex.Message);
            }
            finally
            {
                ////_tcsResponseOk.Task.Dispose();
                ////_tcsResponseOk = null;
                ////_sem.Release();
            }
        }

        public async Task<Result<Unit>> DisconnectAsync(Guid id)
        {
            _cts.Cancel();
            _cts.Dispose();
            //// TODO: Dispose _apiRequests tcs

            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
            return Result<Unit>.Ok(Unit.Default);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

    }
}
