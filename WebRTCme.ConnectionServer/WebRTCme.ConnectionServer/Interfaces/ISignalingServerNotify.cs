using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.ConnectionServer
{
    public interface ISignalingServerNotify
    {
        Task OnPeerJoined(Guid peerId, string peerName, string room);

        Task OnPeerLeft(Guid peerId);

        Task OnPeerSdpAsync(Guid peerId, string peerSdp);

        Task OnPeerIceAsync(Guid peerId, string peerIce);

    }
}
