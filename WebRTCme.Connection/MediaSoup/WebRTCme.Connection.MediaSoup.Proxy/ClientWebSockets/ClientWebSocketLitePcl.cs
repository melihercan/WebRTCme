﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketClientLite.PCL;

namespace WebRTCme.Connection.MediaSoup.ClientWebSockets
{
    class ClientWebSocketLitePcl : IClientWebSocket
    {
        private class ClientWebSocketOptionsLitePcl : IClientWebSocketOptions
        {
            readonly MessageWebSocketRx _baseWebSocket;

            public ClientWebSocketOptionsLitePcl(MessageWebSocketRx baseWebSocket)
            {
                _baseWebSocket = baseWebSocket;
            }

            public bool IgnoreServerCertificateErrors 
            { 
                get => _baseWebSocket.IgnoreServerCertificateErrors; 
                set => _baseWebSocket.IgnoreServerCertificateErrors = value; 
            }

            public void AddSubProtocol(string subProtocol)
            {
                _baseWebSocket.Subprotocols = _baseWebSocket.Subprotocols.Concat(new string[] { subProtocol });
            }

            public void SetRequestHeader(string headerName, string headerValue)
            {
                _baseWebSocket.Headers.Add(headerName, headerValue);
            }
        }

        readonly MessageWebSocketRx _baseWebSocket;
        readonly IClientWebSocketOptions _options;

        public ClientWebSocketLitePcl()
        {
            _baseWebSocket = new MessageWebSocketRx
            {
                Headers = new Dictionary<string, string> { { "Pragma", "no-cache" }, { "Cache-Control", "no-cache" } },
                TlsProtocolType = SslProtocols.Tls12,
                Subprotocols = new string[] { "protoo", "Sec-WebSocket-Protocol" }
            };
            _options = new ClientWebSocketOptionsLitePcl(_baseWebSocket);
        }

        public IClientWebSocketOptions Options => _options;

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, 
            CancellationToken cancellationToken)
        {
            return _baseWebSocket.DisconnectAsync();
        }

        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            TaskCompletionSource<Unit> tcs = new();

            using (cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
            }))
            {
                var connectionDisposable = _baseWebSocket.ConnectionStatusObservable.Subscribe(
                    status =>
                    {
                        if (status == IWebsocketClientLite.PCL.ConnectionStatus.Disconnected ||
                            status == IWebsocketClientLite.PCL.ConnectionStatus.Aborted ||
                            status == IWebsocketClientLite.PCL.ConnectionStatus.ConnectionFailed)
                        {
                            tcs.TrySetException(new WebSocketException("Connection failed"));
                        }
                    },
                    ex =>
                    {
                        tcs.TrySetException(ex);
                    },
                    () =>
                    {
                        tcs.TrySetResult(Unit.Default);
                    });

                await _baseWebSocket.ConnectAsync(uri);
                try
                {
                    _ = await tcs.Task;
                }
                catch (WebSocketException)
                {
                    connectionDisposable.Dispose();
                    throw;
                }
                catch 
                {
                    throw;
                }
                connectionDisposable.Dispose();
            }
        }

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
