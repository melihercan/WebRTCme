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

        Task OnPeerLeft(string roomName, string peerUserName);

        Task OnPeerSdpOffered(string roomName, string peerUserName, string peerSdp);

        Task OnPeerSdpAnswered(string roomName, string peerUserName, string peerSdp);

        Task OnPeerIceCandidate(string roomName, string peerUserName, string peerIce);
    }
}
