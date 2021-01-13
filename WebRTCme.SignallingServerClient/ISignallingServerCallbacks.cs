using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public interface ISignallingServerCallbacks
    {
        Task OnRoomJoined(string roomName, string pairUserName);

        Task OnRoomLeft(string roomName, string pairUserName);

        Task OnRoomStarted(string roomName, RTCIceServer[] iceServers);

        Task OnRoomStopped(string roomName);

        Task OnSdpOffered(string roomName, string pairUserName, string sdp);

        Task OnSdpAnswered(string roomName, string pairUserName, string sdp);

    }
}
