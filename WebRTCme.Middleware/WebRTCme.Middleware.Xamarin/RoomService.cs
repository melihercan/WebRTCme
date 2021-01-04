using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRtcMiddlewareXamarin
{
    internal class RoomService : IRoomService
    {
        public Task CreateRoomAsync(TurnServer turnServer, string roomId, string clientId)
        {
            throw new NotImplementedException();
        }

        public Task JoinRoomAsync(TurnServer turnServer, string roomId, string clientId)
        {
            throw new NotImplementedException();
        }
    }
}
