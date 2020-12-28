using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.SignallingServer.TurnServerService;

namespace WebRTCme.SignallingServer.Hubs
{
    public class RoomHub : Hub
    {
        private readonly ITurnServerClient _turnServerClient;

        public RoomHub(TurnServerClientFactory turnServerClientFactory)
        {
            _turnServerClient = turnServerClientFactory.Create(TurnServer.Xirsys);
        }

        public async Task NewRoom(string clientId, string roomId)
        {
            var iceServers = await _turnServerClient.GetIceServers();
        }
    }
}
