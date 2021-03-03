using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerProxy
{
    public interface ISignallingServerProxy : IAsyncDisposable
    {
        Task<Result<string[]>> GetTurnServerNamesAsync();

        Task<Result<RTCIceServer[]>> GetIceServersAsync(string turnServerName);

        //Task<Result<Unit>> ReserveRoomAsync(string turnServerName, string roomName, string adminUserName, 
        //    string[] participantUserNames);

        //Task<Result<Unit>> FreeRoomAsync(string turnServerName, string roomName, string adminUserName);

        //Task<Result<Unit>> AddParticipantAsync(string turnServerName, string roomName, string participantUserName);

        //Task<Result<Unit>> RemoveParticipantAsync(string turnServerName, string roomName, string participantUserName);

        Task<Result<Unit>> JoinRoomAsync(string turnServerName, string roomName, string userName);
        
        Task<Result<Unit>> LeaveRoomAsync(string turnServerName, string roomName, string userName);

        Task<Result<Unit>> OfferSdpAsync(string turnServerName, string roomName, string peerUserName, string sdp);

        Task<Result<Unit>> AnswerSdpAsync(string turnServerName, string roomName, string peerUserName, string sdp);

        Task<Result<Unit>> IceCandidateAsync(string turnServerName, string roomName, string peerUserName, string ice);

    }
}
