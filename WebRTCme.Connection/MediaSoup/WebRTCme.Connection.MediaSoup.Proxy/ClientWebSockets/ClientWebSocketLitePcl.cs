using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using IWebsocketClientLite;
using WebsocketClientLite;

namespace WebRTCme.Connection.MediaSoup.ClientWebSockets
{
    class ClientWebSocketLitePcl : IClientWebSocket
    {
        private class ClientWebSocketOptionsLitePcl : IClientWebSocketOptions
        {
            public bool IgnoreServerCertificateErrors { get; set; }
            public List<string> Subprotocols { get; } = new() { "protoo", "Sec-WebSocket-Protocol" };
            public Dictionary<string, string> Headers { get; } = new()
            {
                { "Pragma", "no-cache" },
                { "Cache-Control", "no-cache" }
            };

            bool IClientWebSocketOptions.IgnoreServerCertificateErrors
            {
                get => IgnoreServerCertificateErrors;
                set => IgnoreServerCertificateErrors = value;
            }

            public void AddSubProtocol(string subProtocol)
            {
                Subprotocols.Add(subProtocol);
            }

            public void SetRequestHeader(string headerName, string headerValue)
            {
                Headers[headerName] = headerValue;
            }
        }

        readonly ClientWebSocketOptionsLitePcl _options;
        ClientWebSocketRx _baseWebSocket;
        Channel<string> _channel;
        IDisposable _receiverDisposable;

        public ClientWebSocketLitePcl()
        {
            _options = new ClientWebSocketOptionsLitePcl();
        }

        public IClientWebSocketOptions Options => _options;

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, 
            CancellationToken cancellationToken)
        {
            _receiverDisposable?.Dispose();
            _channel.Writer.Complete();
            return Task.CompletedTask;
        }

        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            // Unbounded: this is the socket's receive buffer, and the writer below runs inside
            // an Rx OnNext, so it cannot wait for space. Bounded at 5 it dropped messages --
            // the server bursts a newConsumer/newDataConsumer pair per existing peer right
            // after join, faster than the handlers drain them, and a discarded response left
            // its ApiAsync waiting forever.
            _channel = Channel.CreateUnbounded<string>();
            TaskCompletionSource<Unit> tcs = new();

            _baseWebSocket = new ClientWebSocketRx
            {
                Headers = new Dictionary<string, string>(_options.Headers),
                TlsProtocolType = SslProtocols.Tls12,
                Subprotocols = _options.Subprotocols.ToArray(),
                IgnoreServerCertificateErrors = _options.IgnoreServerCertificateErrors,
            };

            using (cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
            }))
            {
                _receiverDisposable = _baseWebSocket.WebsocketConnectWithStatusObservable(uri)
                    .Subscribe(
                        tuple =>
                        {
                            Console.WriteLine($"======> Connect: {tuple.state}");
                            if (tuple.state == ConnectionStatus.WebsocketConnected)
                            {
                                tcs.TrySetResult(Unit.Default);
                            }
                            else if (tuple.state == ConnectionStatus.Aborted ||
                                tuple.state == ConnectionStatus.ConnectionFailed)
                            {
                                tcs.TrySetException(new WebSocketException("Connection failed"));
                            }
                            else if (tuple.state == ConnectionStatus.DataframeReceived
                                && tuple.dataframe is not null)
                            {
                                if (!_channel.Writer.TryWrite(tuple.dataframe.Message))
                                {
                                    // Unbounded, so this only happens once the channel is
                                    // completed -- worth reporting rather than asserting.
                                    Console.WriteLine(
                                        "ERROR: dropped an incoming websocket message");
                                }
                            }
                        },
                        ex =>
                        {
                            tcs.TrySetException(ex);
                            _channel.Writer.Complete(ex);
                        },
                        () =>
                        {
                            tcs.TrySetResult(Unit.Default);
                            _channel.Writer.TryComplete();
                        });

                try
                {
                    _ = await tcs.Task;
                }
                catch
                {
                    _receiverDisposable.Dispose();
                    throw;
                }
            }
        }

        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, 
            CancellationToken cancellationToken)
        {
            var message = await _channel.Reader.ReadAsync(cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(message);
            bytes.CopyTo(buffer.Array, 0);
            return new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Text, true);
        }

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, 
            CancellationToken cancellationToken)
        {
            return _baseWebSocket.Sender.SendText(Encoding.UTF8.GetString(buffer.ToArray()));
        }
    }
}
