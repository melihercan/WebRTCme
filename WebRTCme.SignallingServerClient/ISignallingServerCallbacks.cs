using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public interface ISignallingServerCallbacks
    {
        Task OnRoomStarted(string roomName, RTCIceServer[] iceServers);

        Task OnRoomStopped(string roomName);

        Task OnPeerJoined(string roomName, string peerUserName);

        Task OnPeerJoined(string roomName, string peerUserName, RTCIceServer[] iceServers);

        Task OnPeerLeft(string roomName, string peerUserName);

        Task OnPeerSdpOffered(string roomName, string peerUserName, string peerSdp);

        Task OnPeerSdpAnswered(string roomName, string peerUserName, string peerSdp);

        Task OnPeerIceCandidate(string roomName, string peerUserName, string peerIce);



        Task OnPeerJoined(string turnServerName, string roomName, string peerUserName, RTCIceServer[] iceServers);

        Task OnPeerLeft(string turnServerName, string roomName, string peerUserName);

        Task OnPeerSdpOffered(string turnServerName, string roomName, string peerUserName, string peerSdp);

        Task OnPeerSdpAnswered(string turnServerName, string roomName, string peerUserName, string peerSdp);

        Task OnPeerIceCandidate(string turnServerName, string roomName, string peerUserName, string peerIce);

    }
}
