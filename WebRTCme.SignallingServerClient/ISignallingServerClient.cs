using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public interface ISignallingServerClient
    {
        Task InitializeAsync();
        Task CleanupAsync();

        Task<RTCIceServer[]> CreateRoomAsync(string roomId, string clientId);
        Task<RTCIceServer[]> JoinRoomAsync(string roomId, string clientId);


    }
}
