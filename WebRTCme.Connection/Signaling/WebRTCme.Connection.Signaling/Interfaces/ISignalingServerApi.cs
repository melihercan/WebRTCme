using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Utilme;

namespace WebRTCme.Connection.Signaling
{
    public interface ISignalingServerApi : IAsyncDisposable
    {
        Task<Result<RTCIceServer[]>> GetIceServersAsync();

        Task<Result<Unit>> JoinAsync(Guid id, string name, string room);

        Task<Result<Unit>> LeaveAsync(Guid id);

        Task<Result<Unit>> SdpAsync(Guid peerId, string sdp);

        Task<Result<Unit>> IceAsync(Guid peerId, string ice);

        Task<Result<Unit>> MediaAsync(Guid id, bool videoMuted, bool audioMuted, bool speaking);

        event ISignalingServerNotify.PeerJoinedDelegateAsync PeerJoinedEventAsync;
        event ISignalingServerNotify.PeerLeftDelegateAsync PeerLeftEventAsync;
        event ISignalingServerNotify.PeerSdpAsyncDelegateAsync PeerSdpEventAsync;
        event ISignalingServerNotify.PeerIceAsyncDelegateAsync PeerIceEventAsync;
        event ISignalingServerNotify.PeerMediaAsyncDelegateAsync PeerMediaEventAsync;
    }
}
