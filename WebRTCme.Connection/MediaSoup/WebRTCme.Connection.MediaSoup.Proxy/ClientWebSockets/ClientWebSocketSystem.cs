using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebRTCme.Connection.MediaSoup.ClientWebSockets
{
    class ClientWebSocketSystem : IClientWebSocket
    {
        private class ClientWebSocketOptionsSystem : IClientWebSocketOptions
        {
            readonly ClientWebSocket _baseWebSocket;
            bool _ignoreServerCertificateErrors;

            public ClientWebSocketOptionsSystem(ClientWebSocket baseWebSocket)
            {
                _baseWebSocket = baseWebSocket;
            }

            public bool IgnoreServerCertificateErrors 
            { 
                get => _ignoreServerCertificateErrors; 
                set 
                {
#if NETSTANDARD2_0
                    throw new NotImplementedException();
#else
                    _baseWebSocket.Options.RemoteCertificateValidationCallback = delegate { return value; };
                    _ignoreServerCertificateErrors = value;
#endif
                }
            }

            public void AddSubProtocol(string subProtocol)
            {
                _baseWebSocket.Options.AddSubProtocol(subProtocol);
            }

            public void SetRequestHeader(string headerName, string headerValue)
            {
                _baseWebSocket.Options.SetRequestHeader(headerName, headerValue);
            }
        }

        readonly ClientWebSocket _baseWebSocket;
        readonly IClientWebSocketOptions _options;

        public ClientWebSocketSystem()
        {
            _baseWebSocket = new();
            _options = new ClientWebSocketOptionsSystem(_baseWebSocket);
        }

        public IClientWebSocketOptions Options => _options;

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription,
            CancellationToken cancellationToken) => 
                _baseWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) =>
            _baseWebSocket.ConnectAsync(uri, cancellationToken);

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer,
            CancellationToken cancellationToken) =>
                _baseWebSocket.ReceiveAsync(buffer, cancellationToken);

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
            CancellationToken cancellationToken) =>
                _baseWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
    }
}
