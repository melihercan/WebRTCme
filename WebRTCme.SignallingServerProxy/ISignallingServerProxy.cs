using System;
using System.Reactive;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerProxy
{
    public interface ISignallingServerProxy : IAsyncDisposable
    {
        delegate Task JoinedOrLeftCallbackHandler(string turnServerName, string roomName, string peerUserName);
        delegate Task SdpOrIceCallbackHandler(string turnServerName, string roomName, string peerUserName, 
            string sdpOrIce);

        Task<(SignallingServerResult, string[])> GetTurnServerNamesAsync();

        Task<(SignallingServerResult, RTCIceServer[])> GetIceServersAsync(string turnServerName);

        Task<SignallingServerResult> JoinRoomAsync(string turnServerName, string roomName, string userName);
        
        Task<SignallingServerResult> LeaveRoomAsync(string turnServerName, string roomName, string userName);

        // sdp is JSON of RTCSessionDescriptionInit.
        Task<SignallingServerResult> SdpAsync(string turnServerName, string roomName, string peerUserName, string sdp);

        // ice is JSON of RTCIceCandidateInit.
        Task<SignallingServerResult> IceCandidateAsync(string turnServerName, string roomName, string peerUserName, string ice);

        event JoinedOrLeftCallbackHandler OnPeerJoinedAsyncEvent;
        event JoinedOrLeftCallbackHandler OnPeerLeftAsyncEvent;
        event SdpOrIceCallbackHandler OnPeerSdpAsyncEvent;
        event SdpOrIceCallbackHandler OnPeerIceAsyncEvent;

    }
}
