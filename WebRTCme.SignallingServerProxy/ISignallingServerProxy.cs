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

        Task<Result<Unit>> JoinRoomAsync(string turnServerName, string roomName, string userName);
        
        Task<Result<Unit>> LeaveRoomAsync(string turnServerName, string roomName, string userName);

        // sdp is JSON of RTCSessionDescriptionInit.
        Task<Result<Unit>> SdpAsync(string turnServerName, string roomName, string peerUserName, string sdp);

        // ice is JSON of RTCIceCandidateInit.
        Task<Result<Unit>> IceCandidateAsync(string turnServerName, string roomName, string peerUserName, string ice);

    }
}
