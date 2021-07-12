using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Connection
{
    public interface IConnection
    {

        IObservable<PeerResponse> ConnectionRequest(Guid id, string name, string room); 

        Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack);

        Task<IRTCStatsReport> GetStats(Guid id);
      
    }
}
