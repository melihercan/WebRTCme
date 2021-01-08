using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.SignallingServerClient
{
    public interface ISignallingServerClient
    {
        Task InitializeAsync(bool bypassSslCertificateError = false);
        Task CleanupAsync();

        Task<RTCIceServer[]> CreateRoomAsync(TurnServer turnServer, string roomId, string clientId);
        Task<RTCIceServer[]> JoinRoomAsync(TurnServer turnServer, string roomId, string clientId);


    }
}
