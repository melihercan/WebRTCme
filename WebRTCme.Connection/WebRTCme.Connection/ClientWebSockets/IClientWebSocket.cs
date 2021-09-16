using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebRTCme.Connection.ClientWebSockets
{
    public interface IClientWebSocket
    {
        public IClientWebSocketOptions Options { get; set; }

        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    }
}
