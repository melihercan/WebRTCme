using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Connection.Interfaces
{
    public interface IConnection
    {

        IObservable<PeerResponse> ConnectionRequest(string name, string room); 

        Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack);

       
    }
}
