using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Connection
{
    public interface IConnection
    {

        IObservable<PeerResponse> ConnectionRequest(UserContext userContext); 

        Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack);

        Task<IRTCStatsReport> GetStats(Guid id);
      
    }
}
