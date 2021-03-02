using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public interface ISignallingServerCallbacks
    {
        //Task OnRoomStarted(string roomName, RTCIceServer[] iceServers);

        //Task OnRoomStopped(string roomName);

        //Task OnPeerJoined(string roomName, string peerUserName);

        //Task OnPeerJoined(string roomName, string peerUserName, RTCIceServer[] iceServers);

        //Task OnPeerLeft(string roomName, string peerUserName);

        //Task OnPeerSdpOffered(string roomName, string peerUserName, string peerSdp);

        //Task OnPeerSdpAnswered(string roomName, string peerUserName, string peerSdp);

        //Task OnPeerIceCandidate(string roomName, string peerUserName, string peerIce);



        Task OnPeerJoinedAsync(string turnServerName, string roomName, string peerUserName);

        Task OnPeerLeftAsync(string turnServerName, string roomName, string peerUserName);

        Task OnPeerSdpOfferedAsync(string turnServerName, string roomName, string peerUserName, string peerSdp);

        Task OnPeerSdpAnsweredAsync(string turnServerName, string roomName, string peerUserName, string peerSdp);

        Task OnPeerIceCandidateAsync(string turnServerName, string roomName, string peerUserName, string peerIce);

    }
}
