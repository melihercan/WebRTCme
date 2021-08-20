using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilme;
using WebRTCme.Connection.MediaSoup;

namespace WebRTCme.Connection.MediaSoup.Proxy.Stub
{
    class MediaSoupStub : IMediaSoupServerApi
    {
        readonly ClientWebSocket _webSocket = new();
        readonly ArraySegment<byte> _rxBuffer = new(new byte[16384]);

        TaskCompletionSource<ProtooResponseOk> _tcsResponseOk;
        TaskCompletionSource<ProtooRequest> _tcsRequest;
        TaskCompletionSource<ProtooNotification> _tcsNotification;
        CancellationTokenSource _cts;
        string _mediaSoupServerBaseUrl;
        static uint _counter;
        SemaphoreSlim _sem = new(1);

        public event IMediaSoupServerNotify.NotifyDelegateAsync NotifyEventAsync;
        public event IMediaSoupServerNotify.RequestDelegateAsync RequestEventAsync;

        public MediaSoupStub(IConfiguration configuration, IWebRtc webRtc, ILogger<MediaSoupStub> logger,
            IJSRuntime jsRuntime = null)
        {
            _mediaSoupServerBaseUrl = configuration["MediaSoupServer:BaseUrl"];
            Registry.WebRtc = webRtc;
            Registry.Logger = logger;
            Registry.JsRuntime = jsRuntime;
        }

        public async Task<Result<Unit>> ConnectAsync(Guid id, string name, string room)
        {
            _cts = new();

            var uri = new Uri(new Uri(_mediaSoupServerBaseUrl),
                $"?roomId={room}" +
                $"&peerId={name}");
            _webSocket.Options.AddSubProtocol("protoo");
            _webSocket.Options.AddSubProtocol("Sec-WebSocket-Protocol");
            await _webSocket.ConnectAsync(uri, _cts.Token);

            // Task handling incoming requests.
            _ = Task.Run(async () => 
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        _tcsRequest = new();
                        var request = await _tcsRequest.Task;

                        await RequestEventAsync?.Invoke(request.Method, request.Data,
                            // accept
                            async (data) =>
                            {
                                try
                                {
                                    await _sem.WaitAsync();
                                    var response = new ProtooResponse
                                    {
                                        Response = true,
                                        Id = request.Id,
                                        Ok = true,
                                        Data = data
                                    };
                                    await _webSocket.SendAsync(
                                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                                            JsonSerializer.Serialize(response, 
                                                JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions))),
                                        WebSocketMessageType.Text,
                                        true,
                                        _cts.Token);
                                    Registry.Logger.LogInformation($"<======= OnRequestAsync Response: {request.Method}");
                                }
                                finally
                                {
                                    _sem.Release();
                                }
                            },
                            // reject
                            async (error, errorReason) =>
                            {
                                try
                                {
                                    await _sem.WaitAsync();
                                    var response = new ProtooResponse
                                    {
                                        Response = true,
                                        Id = request.Id,
                                        Ok = true,
                                        ErrorCode = error,
                                        ErrorReason = errorReason
                                    };
                                    await _webSocket.SendAsync(
                                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                                            JsonSerializer.Serialize(request, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions))),
                                        WebSocketMessageType.Text,
                                        true,
                                        _cts.Token);
                                    Registry.Logger.LogInformation($"<======= OnRequestAsync Error: {request.Method}");
                                }
                                finally
                                {
                                    _sem.Release();
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
                        _tcsNotification = new();
                        var notification = await _tcsNotification.Task;
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

            // Task handling incoming messages and dispatching them.
            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var bytes = await _webSocket.ReceiveAsync(_rxBuffer, _cts.Token);
                        var json = Encoding.UTF8.GetString(_rxBuffer.Array, 0, bytes.Count);
  Console.WriteLine($">>>>>>>>>>>>> INCOMING MSG: {json}");
                        var jsonDocument = JsonDocument.Parse(json);
                        if (jsonDocument.RootElement.TryGetProperty("response", out _))
                        {
                            var ok = jsonDocument.RootElement.TryGetProperty("ok", out _);
                            if (ok)
                            {
                                var responseOk = JsonSerializer.Deserialize<ProtooResponseOk>(json,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                _tcsResponseOk?.SetResult(responseOk);
                            }
                            else
                            {
                                var responseError = JsonSerializer.Deserialize<ProtooResponseError>(json,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                                Registry.Logger.LogError(responseError.ErrorReason);
                                _tcsResponseOk?.SetException(new Exception($"{responseError.ErrorReason}"));
                            }
                        }
                        else if (jsonDocument.RootElement.TryGetProperty("request", out _))
                        {
                            var request = JsonSerializer.Deserialize<ProtooRequest>(json,
                                JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                            _tcsRequest?.SetResult(request);
                        }
                        else if (jsonDocument.RootElement.TryGetProperty("notification", out _))
                        {
                            var notification = JsonSerializer.Deserialize<ProtooNotification>(json,
                                    JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                            _tcsNotification?.SetResult(notification);
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


        public async Task<Result<object>> CallAsync(string method, object data)
        {
            Registry.Logger.LogInformation($"######## CallAsync: {method}");
            try
            {
                await _sem.WaitAsync();
                _tcsResponseOk = new();

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

                var response = await _tcsResponseOk.Task;
                if (response.Id != request.Id)
                    throw new Exception($"request.Id:{request.Id} and response.Id:{response.Id} are different!");

                return Result<object>.Ok(response.Data);
            }
            catch (Exception ex)
            {
                return Result<object>.Error(ex.Message);
            }
            finally
            {
                _tcsResponseOk.Task.Dispose();
                _tcsResponseOk = null;
                _sem.Release();
            }
        }

        public async Task<Result<Unit>> DisconnectAsync(Guid id)
        {
            _cts.Cancel();
            _cts.Dispose();
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
            return Result<Unit>.Ok(Unit.Default);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

    }
}
