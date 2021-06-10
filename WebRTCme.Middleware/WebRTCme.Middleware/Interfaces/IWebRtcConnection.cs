using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware.Models;
using WebRTCme.SignallingServerProxy;

namespace WebRTCme.Middleware
{
    public interface IWebRtcConnection
    {
        //// TODO: FIND ANOTHER SOLUTION TO PASS THIS TO WebRtcConnection.
        ISignallingServerProxy SignallingServerProxy { get; set; }

        ConnectionContext GetConnectionContext(string turnServerName, string roomName);

        Task CreateOrDeletePeerConnectionAsync(string turnServerName, string roomName,
            string peerUserName, bool isInitiator, bool isDelete = false);

        IObservable< PeerResponseParameters> ConnectionRequest(ConnectionRequestParameters request);

        Task ReplaceOutgoingVideoTracksAsync(string turnServerName, string roomName, IMediaStreamTrack newVideoTrack);
    }
}
