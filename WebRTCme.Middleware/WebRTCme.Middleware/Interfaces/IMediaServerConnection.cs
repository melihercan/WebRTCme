using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IMediaServerConnection : IAsyncDisposable
    {
        Task<string[]> GetMediaServerNamesAsync();

        IObservable<PeerResponseParameters> ConnectionRequest(ConnectionRequestParameters request);

        Task ReplaceOutgoingVideoTracksAsync(string turnServerName, string roomName, IMediaStreamTrack newVideoTrack);
    }
}
