using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IRoomService
    {
        Task CreateRoomAsync(TurnServer turnServer, string roomId, string clientId);

        Task JoinRoomAsync(TurnServer turnServer, string roomId, string clientId);
    }
}
