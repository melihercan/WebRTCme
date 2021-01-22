using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IRoomService : IAsyncDisposable
    {
        Task<string[]> GetTurnServerNames();


        IObservable<PeerCallbackParameters> JoinRoomRequest(JoinRoomRequestParameters request);
      

    }
}
