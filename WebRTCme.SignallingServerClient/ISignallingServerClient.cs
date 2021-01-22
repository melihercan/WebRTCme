using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public interface ISignallingServerClient : IAsyncDisposable
    {
        Task<Result<string[]>> GetTurnServerNames();

        Task<Result<Unit>> ReserveRoom(string turnServerName, string roomName, string adminUserName, 
            string[] participantUserNames);

        Task<Result<Unit>> FreeRoom(string turnServerName, string roomName, string adminUserName);

        Task<Result<Unit>> AddParticipant(string turnServerName, string roomName, string participantUserName);

        Task<Result<Unit>> RemoveParticipant(string turnServerName, string roomName, string participantUserName);

        Task<Result<Unit>> JoinRoom(string turnServerName, string roomName, string userName);
        
        Task<Result<Unit>> LeaveRoom(string turnServerName, string roomName, string userName);

        Task<Result<Unit>> OfferSdp(string turnServerName, string roomName, string pairUserName, string sdp);

        Task<Result<Unit>> AnswerSdp(string turnServerName, string roomName, string pairUserName, string sdp);

        Task<Result<Unit>> IceCandidate(string turnServerName, string roomName, string pairUserName, string ice);
    }
}
