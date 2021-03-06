using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerProxy
{
    public interface ISignallingServerCallbacks
    {
        Task OnPeerJoinedAsync(string turnServerName, string roomName, string peerUserName);

        Task OnPeerLeftAsync(string turnServerName, string roomName, string peerUserName);

        // peerDdp is JSON of RTCSessionDescriptionInit.
        Task OnPeerSdpAsync(string turnServerName, string roomName, string peerUserName, string peerSdp);

        // peerIce is JSON of RTCIceCandidateInit.
        Task OnPeerIceCandidateAsync(string turnServerName, string roomName, string peerUserName, string peerIce);

    }
}
