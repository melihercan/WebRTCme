using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Connection.Services
{
    class MediaSoupConnection : IConnection
    {
        public IObservable<PeerResponse> ConnectionRequest(Guid id, string name, string room)
        {
            throw new NotImplementedException();
        }

        public Task<IRTCStatsReport> GetStats(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack)
        {
            throw new NotImplementedException();
        }
    }
}
