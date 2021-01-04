using Ardalis.Result;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.Middleware;
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
            Result<RTCIceServer[]> result = null;

            try
            {
                var iceServers = await _turnServerClient.GetIceServersAsync();
                result = Result<RTCIceServer[]>.Success(iceServers);
            }
            catch (Exception ex)
            {
                result = Result<RTCIceServer[]>.Error(new string[] { ex.Message });
            }
            
            await Clients.Caller.SendAsync("SignallingServerHubCallback_RoomResponse", result);
        }

        //public async Task CreateRoomByTurnServerName(TurnServer turnServer, string roomId, string clientId)
        //{

        //}

  //      public async Task CreateRoomByTurnServerUrlWithRoom(string turnServerUrl, string roomId, string clientId)
    //    {

      //  }

//        public async Task JoinRoom(string )

    }
}
