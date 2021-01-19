using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public interface ISignallingServerClient
    {
        Task InitializeAsync(ISignallingServerCallbacks signallingServerCallbacks);
        Task CleanupAsync();

        Task JoinRoomAsync(string roomName, string userName);
        
        Task LeaveRoomAsync(string roomName, string userName);

        Task StartRoomAsync(string roomName, string userName, TurnServer turnServer);

        Task StopRoomAsync(string roomName, string userName);

        Task OfferSdpAsync(string roomName, string pairUserName, string sdp);

        Task AnswerSdpAsync(string roomName, string pairUserName, string sdp);

        Task IceCandidateAsync(string roomName, string pairUserName, string ice);

    }
}
