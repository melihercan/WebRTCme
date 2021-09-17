using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebRTCme.Connection.MediaSoup.ClientWebSockets
{
    class ClientWebSocketLitePcl : IClientWebSocket
    {
        public IClientWebSocketOptions Options { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
