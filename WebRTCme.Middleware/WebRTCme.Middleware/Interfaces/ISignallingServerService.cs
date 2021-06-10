using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.SignallingServerProxy;

namespace WebRTCme.Middleware
{
    public interface ISignallingServerService : /*IAsyncInitialization,*/ IAsyncDisposable
    {
        Task<string[]> GetTurnServerNamesAsync();

        //IObservable<PeerResponseParameters> ConnectionRequest(ConnectionRequestParameters request);

        //Task ReplaceOutgoingVideoTracksAsync(string turnServerName, string roomName, IMediaStreamTrack newVideoTrack);

    }
}
