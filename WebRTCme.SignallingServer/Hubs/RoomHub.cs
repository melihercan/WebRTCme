using Ardalis.Result;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.SignallingServer.Models;
using WebRTCme.SignallingServer.TurnServerService;

namespace WebRTCme.SignallingServer.Hubs
{
    public class RoomHub : Hub
    {
        private TurnServerClientFactory _turnServerClientFactory;
        private ITurnServerClient _turnServerClient;

        private static List<Client> _clients = new();

        public RoomHub(TurnServerClientFactory turnServerClientFactory)
        {
            _turnServerClientFactory = turnServerClientFactory;
        }

        public async Task EchoToCaller(string message)
        {
            await Clients.Caller.SendAsync("EchoToCallerResponse", message);
        }

        public Task<Result<RTCIceServer[]>> CreateRoom(TurnServer turnServer, string roomId, string userId)
        {
            if (_clients.Any(client => client.TurnServer == turnServer && client.RoomId == roomId))
            {
                // Room already exist on this server.
                return Task.FromResult(Result<RTCIceServer[]>.Error(
                    new string[] { $"Room:{roomId} has already been created on {turnServer} TURN server" }));
            }

            return HandleRoomRequest(true, turnServer, roomId, userId);
        }

        public Task<Result<RTCIceServer[]>> JoinRoom(TurnServer turnServer, string roomId, string userId)
        {
            return HandleRoomRequest(false, turnServer, roomId, userId);
        }


        private async Task<Result<RTCIceServer[]>> HandleRoomRequest(bool isInitiator, TurnServer turnServer, 
            string roomId, string userId)
        {
            if (_clients.Any(client => client.TurnServer == turnServer && client.RoomId == roomId))
            {
                // Room already exist on this server.
                return Result<RTCIceServer[]>.Error(new string[] { $"Room:{roomId} has already been created on {turnServer} TURN server" });
            }

            //            var x = _clients
            //              .Where(client => client.TurnServer == turnServer && client.RoomId == roomId);
            //.GroupBy(client => new { client.TurnServer, client.RoomId });

            _turnServerClient = _turnServerClientFactory.Create(turnServer);

            try
            {
                var iceServers = await _turnServerClient.GetIceServersAsync();
                _clients.Add(new Client
                {
                    ClientId = Context.ConnectionId,
                    IsInitiator = isInitiator,
                    TurnServer = turnServer,
                    RoomId = roomId,
                    UserId = userId
                });
                return Result<RTCIceServer[]>.Success(iceServers);
            }
            catch (Exception ex)
            {
                return Result<RTCIceServer[]>.Error(new string[] { ex.Message });
            }
        }


        public async Task SendSdpOffer(string sdp)
        {
            await Clients.Others.SendAsync("OnSdpOffer", sdp);
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
