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

        private static List<Room> _rooms = new();
        private static List<Client> _awaitingClients = new(); 

        public RoomHub(TurnServerClientFactory turnServerClientFactory)
        {
            _turnServerClientFactory = turnServerClientFactory;
        }

        public async Task EchoToCaller(string message)
        {
            await Clients.Caller.SendAsync("EchoToCallerResponse", message);
        }

        public async Task JoinRoom(string roomName, string userName)
        {
            var client = new Client
            {
                ConnectionId = Context.ConnectionId,
                RoomName = roomName,
                UserName = userName
            };
            var room = _rooms.Find(room => room.GroupName == roomName);
            if (room is null)
                // Room has not been started yet. Queue the client in waiting list.
                _awaitingClients.Add(client);
            else
            {
                // Room has been started. Add this client to room, send IceServer list and notify all other clients
                // in the group.
                room.Clients.ToList().Add(client);
                await Clients.Caller.SendAsync("OnIceServers", room.IceServers);
                await Clients.GroupExcept(roomName, Context.ConnectionId).SendAsync("OnClientReady", userName);
            }


            //var filteredClients = _rooms.Where(client => client.TurnServer == turnServer && client.GroupName == roomName);
            //if (filteredClients.Count() == 0)
            //    return Task.FromResult(Result<RTCIceServer[]>.Error(
            //        new string[] { $"TURN server {turnServer} has no room {roomName}" }));

            //if (filteredClients.Any(client => client.UserId == userName))
            //    return Task.FromResult(Result<RTCIceServer[]>.Error(
            //        new string[] { $"User {userName} has already connected to TURN server:{turnServer} room:{roomName}" }));

            //return HandleRoomRequest(false, turnServer, roomName, userName);
        }


        public async Task<Result<object>> StartRoom(string roomName, string userName, TurnServer turnServer)
        {
            if (_rooms.Any(room => room.GroupName == roomName))
                return Result<object>.Error(new string[] { $"Room:{roomName} is in use" });

            _turnServerClient = _turnServerClientFactory.Create(turnServer);

            try
            {
                var iceServers = await _turnServerClient.GetIceServersAsync();
                var newRoomClients = _awaitingClients.Where(client => client.RoomName == roomName);
                _awaitingClients.RemoveAll(client => client.RoomName == roomName);
                var room = new Room
                {
                    GroupName = roomName,
                    IceServers = iceServers,
                    InitiatiorUserName = userName,
                    Clients = newRoomClients
                };
                _rooms.Add(room);
                await Clients.Group(roomName).SendAsync("OnIceServers", room.IceServers);

                var except = new List<string>();
                foreach (var client in newRoomClients)
                {
                    except.Add(client.ConnectionId);
                    await Clients.GroupExcept(roomName, except).SendAsync("OnClientReady", client.UserName);
                }

                return Result<object>.Success(null);
            }
            catch (Exception ex)
            {
                return Result<object>.Error(new string[] { ex.Message });
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
