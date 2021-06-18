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

        Task<string[]> GetTurnServerNamesAsync();

        Task<RTCIceServer[]> GetIceServersAsync(string turnServerName);

        Task JoinRoomAsync(string turnServerName, string roomName, string userName);
        
        Task LeaveRoomAsync(string turnServerName, string roomName, string userName);

        // sdp is JSON of RTCSessionDescriptionInit.
        Task SdpAsync(string turnServerName, string roomName, string peerUserName, string sdp);

        // ice is JSON of RTCIceCandidateInit.
        Task IceCandidateAsync(string turnServerName, string roomName, string peerUserName, string ice);

        event JoinedOrLeftCallbackHandler OnPeerJoinedAsyncEvent;
        event JoinedOrLeftCallbackHandler OnPeerLeftAsyncEvent;
        event SdpOrIceCallbackHandler OnPeerSdpAsyncEvent;
        event SdpOrIceCallbackHandler OnPeerIceAsyncEvent;

    }
}
