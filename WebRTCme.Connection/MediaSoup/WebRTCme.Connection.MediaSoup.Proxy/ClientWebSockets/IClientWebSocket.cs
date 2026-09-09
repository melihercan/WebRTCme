using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebRTCme.Connection.MediaSoup.ClientWebSockets
{
    /// <summary>
    /// A protoo transport: whole text messages in, whole text messages out.
    /// </summary>
    /// <remarks>
    /// Message oriented rather than frame oriented on purpose. The two implementations frame
    /// differently -- the system socket hands back partial frames that have to be reassembled,
    /// the pure-managed one has already reassembled by the time it reports a message -- and
    /// leaving that difference to the caller previously meant a message larger than the
    /// caller's buffer was truncated on one platform and threw on the other.
    /// </remarks>
    public interface IClientWebSocket
    {
        public IClientWebSocketOptions Options { get; }

        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

        Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription,
            CancellationToken cancellationToken);

        /// <summary>
        /// Reads the next complete message, however many frames it arrives in.
        /// </summary>
        Task<string> ReceiveMessageAsync(CancellationToken cancellationToken);

        Task SendMessageAsync(string message, CancellationToken cancellationToken);
    }
}
