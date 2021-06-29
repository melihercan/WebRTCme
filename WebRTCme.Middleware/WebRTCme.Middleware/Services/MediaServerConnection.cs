using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware.Services
{
    class MediaServerConnection : IMediaServerConnection
    {
        public Task<string[]> GetMediaServerNamesAsync()
        {
            throw new NotImplementedException();
        }
        public IObservable<PeerResponseParameters> ConnectionRequest(ConnectionRequestParameters request)
        {
            throw new NotImplementedException();
        }



        public Task ReplaceOutgoingVideoTracksAsync(string turnServerName, string roomName, IMediaStreamTrack newVideoTrack)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

    }
}
