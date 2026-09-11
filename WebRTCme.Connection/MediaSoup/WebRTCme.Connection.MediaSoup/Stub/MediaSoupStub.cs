using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Utilme;
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.ClientWebSockets;
using Microsoft.Maui.Devices;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// protoo client: one socket, one reader, one handler queue.
    /// </summary>
    /// <remarks>
    /// Responses are completed straight off the reader so a handler awaiting an API call of its
    /// own can never block the answer it is waiting for, while requests and notifications go
    /// through a single queue that runs them one at a time in arrival order. The previous shape
    /// -- three independent pumps over bounded channels -- let a handler run before the response
    /// that preceded it had been delivered.
    /// </remarks>
    class MediaSoupStub : IMediaSoupServerApi
    {
        // protoo-server gives a peer 30s to answer a request before failing the operation, so
        // waiting materially longer than that for our own requests only hides a dead connection.
        static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

        readonly ClientWebSocketFactory _clientWebSocketFactory;
        readonly string _mediaSoupServerBaseUrl;

        readonly ConcurrentDictionary<uint, TaskCompletionSource<ProtooResponse>> _apiRequests = new();

        IClientWebSocket _webSocket;
        CancellationTokenSource _cts;
        Channel<object> _incoming;
        Task _receiveLoop;
        Task _dispatchLoop;
        int _counter;

        public event IMediaSoupServerNotify.NotifyDelegateAsync NotifyEventAsync;
        public event IMediaSoupServerNotify.RequestDelegateAsync RequestEventAsync;

        public MediaSoupStub(ClientWebSocketFactory clientWebSocketFactory, IConfiguration configuration,
            IWebRtc webRtc, ILogger<MediaSoupStub> logger, IJSRuntime jsRuntime = null)
        {
            _clientWebSocketFactory = clientWebSocketFactory;
            _mediaSoupServerBaseUrl = configuration["MediaSoupServer:BaseUrl"];
            Registry.WebRtc = webRtc;
            Registry.Logger = logger;
            Registry.JsRuntime = jsRuntime;
        }

        public async Task<Result<Unit>> ConnectAsync(Guid id, string name, string room)
        {
            // A socket is single use: a closed one cannot be reopened, so every connect gets a
            // fresh one. Anything left over from a previous call goes first.
            await TearDownAsync();

            _cts = new();
            _webSocket = CreateWebSocket();

            var uri = new Uri(new Uri(_mediaSoupServerBaseUrl),
                $"?roomId={room}" +
                $"&peerId={name}");
            _webSocket.Options.AddSubProtocol("protoo");
            SetOriginHeader(uri);

            try
            {
                await _webSocket.ConnectAsync(uri, _cts.Token);
            }
            catch (Exception ex)
            {
                // Report it. Swallowing this used to leave every later send failing for reasons
                // that had nothing to do with the actual problem.
                Log($"Cannot connect to the mediasoup server at {uri}: {ex.Message}");
                await TearDownAsync();
                return Result<Unit>.Error($"Cannot connect to the mediasoup server: {ex.Message}");
            }

            _incoming = Channel.CreateUnbounded<object>(
                new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });

            _receiveLoop = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            _dispatchLoop = Task.Run(() => DispatchLoopAsync(_cts.Token));

            return Result<Unit>.Ok(Unit.Default);
        }


        /// <summary>
        /// Presents an Origin the server will accept.
        /// </summary>
        /// <remarks>
        /// The server rejects a WebSocket whose Origin does not match its own, and treats a
        /// missing one as a mismatch. A browser sets this itself and will not let anyone else
        /// set it, so this is only for the platforms where we drive the socket ourselves; the
        /// server compares scheme and host only, so deriving it from the server address is
        /// enough.
        /// </remarks>
        void SetOriginHeader(Uri serverUri)
        {
            var scheme = serverUri.Scheme == Uri.UriSchemeWss ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

            try
            {
                _webSocket.Options.SetRequestHeader("Origin", $"{scheme}://{serverUri.Host}");
            }
            catch (Exception ex)
            {
                // Blazor: the browser owns this header and forbids setting it, which is fine
                // because the browser is already sending the right one.
                Registry.Logger.LogInformation($"Origin header not set: {ex.Message}");
            }
        }

        IClientWebSocket CreateWebSocket()
        {
            // The demo signaling servers are reachable over a local IP with a self-signed
            // certificate, which the system websocket refuses. Debug builds fall back to the
            // pure-managed socket, which can be told to accept it; release builds always
            // validate. iOS needs this as much as Android does -- without it the handshake
            // fails before any mediasoup code runs.
#if DEBUG
            var bypassSslCertificateError =
                DeviceInfo.Platform == DevicePlatform.Android ||
                DeviceInfo.Platform == DevicePlatform.iOS;
#else
            var bypassSslCertificateError = false;
#endif
            if (!bypassSslCertificateError)
                return _clientWebSocketFactory.Create(ClientWebSocketSelect.System);

            var webSocket = _clientWebSocketFactory.Create(ClientWebSocketSelect.LitePcl);
            webSocket.Options.IgnoreServerCertificateErrors = true;
            return webSocket;
        }

        /// <summary>
        /// Reads messages, answers waiting API calls itself and queues everything else.
        /// </summary>
        async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var json = await _webSocket.ReceiveMessageAsync(cancellationToken);
                    Echo($">>>>>>>>>>>>> INCOMING MSG: {json}");

                    using var jsonDocument = JsonDocument.Parse(json);
                    var root = jsonDocument.RootElement;

                    if (root.TryGetProperty("response", out _))
                    {
                        CompleteApiRequest(json, root);
                    }
                    else if (root.TryGetProperty("request", out _))
                    {
                        await _incoming.Writer.WriteAsync(
                            JsonSerializer.Deserialize<ProtooRequest>(
                                json, JsonHelper.WebRtcJsonSerializerOptions),
                            cancellationToken);
                    }
                    else if (root.TryGetProperty("notification", out _))
                    {
                        await _incoming.Writer.WriteAsync(
                            JsonSerializer.Deserialize<ProtooNotification>(
                                json, JsonHelper.WebRtcJsonSerializerOptions),
                            cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Log($"Receiving from the mediasoup server stopped: {ex}");
            }
            finally
            {
                // Nothing more will arrive, so nobody should keep waiting for it.
                _incoming?.Writer.TryComplete();
                FailPendingApiRequests(
                    new Exception("The connection to the mediasoup server was lost"));
            }
        }

        void CompleteApiRequest(string json, JsonElement root)
        {
            // protoo answers a failed request without an "ok" member at all.
            ProtooResponse response;

            if (root.TryGetProperty("ok", out _))
            {
                var ok = JsonSerializer.Deserialize<ProtooResponseOk>(
                    json, JsonHelper.WebRtcJsonSerializerOptions);
                response = new ProtooResponse
                {
                    Response = ok.Response,
                    Id = ok.Id,
                    Ok = ok.Ok,
                    Data = ok.Data
                };
            }
            else
            {
                var error = JsonSerializer.Deserialize<ProtooResponseError>(
                    json, JsonHelper.WebRtcJsonSerializerOptions);
                response = new ProtooResponse
                {
                    Response = error.Response,
                    Id = error.Id,
                    Ok = error.Ok,
                    ErrorCode = error.ErrorCode,
                    ErrorReason = error.ErrorReason
                };
            }

            if (!response.Ok)
                Log($"The mediasoup server rejected request {response.Id}: {response.ErrorReason}");

            // Gone already when the caller timed out; not an error worth reporting.
            if (_apiRequests.TryRemove(response.Id, out var tcs))
                tcs.TrySetResult(response);
        }

        /// <summary>
        /// Runs request and notification handlers one at a time, in the order they arrived.
        /// </summary>
        async Task DispatchLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var message in _incoming.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        switch (message)
                        {
                            case ProtooRequest request:
                                Echo($"########################## REQUEST: {request.Method}");
                                await OnRequestAsync(request, cancellationToken);
                                break;

                            case ProtooNotification notification:
                                if (NotifyEventAsync is not null)
                                    await NotifyEventAsync.Invoke(notification.Method, notification.Data);
                                break;
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        // One bad message must not stop the rest arriving. Reported to the
                        // console as well as the logger: ILogger output reaches neither logcat
                        // nor the iOS console, so a handler that threw here used to leave no
                        // trace at all and the server simply timed the request out 30s later.
                        Log($"E X C E P T I O N: {ex}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        async Task OnRequestAsync(ProtooRequest request, CancellationToken cancellationToken)
        {
            if (RequestEventAsync is null)
            {
                await SendAsync(new ProtooResponse
                {
                    Response = true,
                    Id = request.Id,
                    Ok = false,
                    ErrorCode = 500,
                    ErrorReason = "No handler is listening for requests"
                }, cancellationToken);
                return;
            }

            // protoo allows exactly one answer per request, so a handler that accepts and then
            // throws must not also produce a rejection.
            var answered = 0;

            await RequestEventAsync.Invoke(request.Method, request.Data,
                async data =>
                {
                    if (Interlocked.Exchange(ref answered, 1) != 0)
                        return;

                    await SendAsync(new ProtooResponse
                    {
                        Response = true,
                        Id = request.Id,
                        Ok = true,
                        Data = data
                    }, cancellationToken);
                    Registry.Logger.LogInformation($"<======= OnRequestAsync Response: {request.Method}");
                },
                async (error, errorReason) =>
                {
                    if (Interlocked.Exchange(ref answered, 1) != 0)
                        return;

                    await SendAsync(new ProtooResponse
                    {
                        Response = true,
                        Id = request.Id,
                        Ok = false,
                        ErrorCode = error,
                        ErrorReason = errorReason
                    }, cancellationToken);
                    Registry.Logger.LogInformation($"<======= OnRequestAsync Error: {request.Method}");
                });
        }

        public async Task<Result<object>> ApiAsync(string method, object data)
        {
            Registry.Logger.LogInformation($"######## CallAsync: {method}");

            var cts = _cts;
            if (_webSocket is null || cts is null || cts.IsCancellationRequested)
                return Result<object>.Error("Not connected to the mediasoup server");

            var request = new ProtooRequest
            {
                Request = true,
                Id = (uint)Interlocked.Increment(ref _counter),
                Method = method,
                Data = data
            };

            TaskCompletionSource<ProtooResponse> tcs =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            _apiRequests[request.Id] = tcs;

            try
            {
                await SendAsync(request, cts.Token);

                // Bounded: without it a dropped or unanswered response left the caller awaiting
                // a completion that never came, and the connection just appeared to stop.
                var response = await tcs.Task.WaitAsync(RequestTimeout, cts.Token);

                if (!response.Ok)
                    return Result<object>.Error(response.ErrorReason);

                return Result<object>.Ok(response.Data);
            }
            catch (TimeoutException)
            {
                return Result<object>.Error(
                    $"The mediasoup server did not answer '{method}' within " +
                    $"{RequestTimeout.TotalSeconds}s");
            }
            catch (Exception ex)
            {
                return Result<object>.Error(ex.Message);
            }
            finally
            {
                _apiRequests.TryRemove(request.Id, out _);
            }
        }

        public async Task<Result<Unit>> NotifyAsync(string method, object data)
        {
            Registry.Logger.LogInformation($"######## NotifyAsync: {method}");

            var cts = _cts;
            if (_webSocket is null || cts is null || cts.IsCancellationRequested)
                return Result<Unit>.Error("Not connected to the mediasoup server");

            try
            {
                // No id and no completion source: a protoo notification is never answered, so
                // there is nothing to correlate a response against and nothing to wait for.
                await SendAsync(new ProtooNotification
                {
                    Notification = true,
                    Method = method,
                    Data = data
                }, cts.Token);

                return Result<Unit>.Ok(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(ex.Message);
            }
        }

        Task SendAsync(object message, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(message, JsonHelper.WebRtcJsonSerializerOptions);
            Echo($"<<<<<<<<<<<<< OUTGOING MSG: {json}");
            return _webSocket.SendMessageAsync(json, cancellationToken);
        }

        public async Task<Result<Unit>> DisconnectAsync(Guid id)
        {
            await TearDownAsync();
            return Result<Unit>.Ok(Unit.Default);
        }

        /// <summary>
        /// Stops the loops, fails anything still waiting and closes the socket. Safe to call
        /// when never connected, and safe to call twice.
        /// </summary>
        async Task TearDownAsync()
        {
            var cts = Interlocked.Exchange(ref _cts, null);
            var webSocket = Interlocked.Exchange(ref _webSocket, null);
            var receiveLoop = Interlocked.Exchange(ref _receiveLoop, null);
            var dispatchLoop = Interlocked.Exchange(ref _dispatchLoop, null);

            if (cts is null && webSocket is null)
                return;

            cts?.Cancel();
            _incoming?.Writer.TryComplete();
            FailPendingApiRequests(new Exception("Disconnected from the mediasoup server"));

            if (webSocket is not null)
            {
                try
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Log($"Closing the mediasoup connection failed: {ex.Message}");
                }
            }

            foreach (var loop in new[] { receiveLoop, dispatchLoop })
            {
                if (loop is null)
                    continue;

                try
                {
                    await loop;
                }
                catch (Exception ex)
                {
                    Log($"A mediasoup connection task ended badly: {ex.Message}");
                }
            }

            cts?.Dispose();
        }

        void FailPendingApiRequests(Exception exception)
        {
            foreach (var id in _apiRequests.Keys)
            {
                if (_apiRequests.TryRemove(id, out var tcs))
                    tcs.TrySetException(exception);
            }
        }

        static void Log(string message)
        {
            Echo(message);
            Registry.Logger?.LogError(message);
        }

        /// <summary>
        /// Writes a line everywhere it might be read from.
        /// </summary>
        /// <remarks>
        /// Console and Debug are not alternatives, because neither reaches every host. A packaged
        /// WinUI app has no console at all, so the protoo traffic logged here was invisible on
        /// Windows; Debug.WriteLine reaches the debugger, or - with no debugger attached - the
        /// Win32 OutputDebugString channel that DebugView and friends can capture. On Android both
        /// arrive in logcat.
        ///
        /// Registry.Logger is a third destination again, and is the one that reaches nothing in
        /// the MAUI apps: they register no logging provider.
        /// </remarks>
        static void Echo(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        public async ValueTask DisposeAsync() => await TearDownAsync();
    }
}
