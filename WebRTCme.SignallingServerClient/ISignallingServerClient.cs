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

        Task JoinRoomAsync(string roomName, string userName);
        Task StartRoomAsync(string roomName, string userName, TurnServer turnServer);

        Task LeaveRoomAsync(string roomName, string userName);




        Task SendSdpOfferAsync(string sdp);


        public Task<string> ExecuteEchoToCaller(string message);

    }
}
