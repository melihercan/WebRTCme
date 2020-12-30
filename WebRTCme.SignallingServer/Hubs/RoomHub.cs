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

        public async Task CreateRoom(string roomId, string clientId)
        {
            var iceServers = await _turnServerClient.GetIceServers();
        }
    }
}
