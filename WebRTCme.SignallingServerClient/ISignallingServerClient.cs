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

        Task<RTCIceServer[]> CreateRoomAsync(TurnServer turnServer, string roomId, string userId);
        Task<RTCIceServer[]> JoinRoomAsync(TurnServer turnServer, string roomId, string userId);


        Task SendSdpOfferAsync(string sdp);


        public Task<string> ExecuteEchoToCaller(string message);

    }
}
