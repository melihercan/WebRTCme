using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Connection.Signaling
{
    public interface ISignalingServerNotify
    {
        delegate Task PeerJoinedDelegateAsync(Guid peerId, string peerName);

        delegate Task PeerLeftDelegateAsync(Guid peerId);

        delegate Task PeerSdpAsyncDelegateAsync(Guid peerId, string peerName, string peerSdp);

        delegate Task PeerIceAsyncDelegateAsync(Guid peerId, string peerIce);

        delegate Task PeerMediaAsyncDelegateAsync(Guid peerId, bool videoMuted, bool audioMuted, bool speaking);


        Task OnPeerJoinedAsync(Guid peerId, string peerName);

        Task OnPeerLeftAsync(Guid peerId);

        Task OnPeerSdpAsync(Guid peerId, string peerName, string peerSdp);

        Task OnPeerIceAsync(Guid peerId, string peerIce);

        Task OnPeerMediaAsync(Guid peerId, bool videoMuted, bool audioMuted, bool speaking);
    }
}
