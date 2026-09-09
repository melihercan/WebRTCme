using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
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

        // Frames arrive in chunks of at most this size; a message is reassembled across as many
        // as it takes, so this bounds a read rather than a message.
        const int ReceiveChunkSize = 16 * 1024;

        readonly ClientWebSocket _baseWebSocket;
        readonly IClientWebSocketOptions _options;

        public ClientWebSocketSystem()
        {
            _baseWebSocket = new();
            _options = new ClientWebSocketOptionsSystem(_baseWebSocket);
        }

        public IClientWebSocketOptions Options => _options;

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription,
            CancellationToken cancellationToken)
        {
            if (_baseWebSocket.State != WebSocketState.Open)
                return Task.CompletedTask;

            return _baseWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
        }

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) =>
            _baseWebSocket.ConnectAsync(uri, cancellationToken);

        public async Task<string> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(ReceiveChunkSize);
            try
            {
                using var message = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await _baseWebSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                        throw new WebSocketException(
                            WebSocketError.ConnectionClosedPrematurely,
                            $"Server closed the connection: {result.CloseStatusDescription}");

                    message.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                return Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public Task SendMessageAsync(string message, CancellationToken cancellationToken) =>
            _baseWebSocket.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                WebSocketMessageType.Text,
                true,
                cancellationToken);
    }
}
