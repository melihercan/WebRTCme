using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Utilme;

namespace WebRTCme.Connection.Signaling.Interfaces
{
    public interface ISignalingServerApi
    {
        Task<Result<RTCIceServer[]>> GetIceServersAsync();

        Task<Result<Unit>> JoinAsync(Guid id, string name, string room);

        Task<Result<Unit>> LeaveAsync(Guid id);

        Task<Result<Unit>> SdpAsync(Guid peerId, string sdp);

        Task<Result<Unit>> IceAsync(Guid peerId, string ice);

        Task<Result<Unit>> MediaAsync(Guid id, MediaContext mediaContext);

    }
}
