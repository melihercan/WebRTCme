using Ardalis.Result;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using WebRTCme.SignallingServer.Models;
using WebRTCme.SignallingServer.TurnServerService;
using WebRTCme.SignallingServerClient;

namespace WebRTCme.SignallingServer.Hubs
{
    public class RoomHub : Hub<ISignallingServerCallbacks>
    {
        private TurnServerClientFactory _turnServerClientFactory;
        private ITurnServerClient _turnServerClient;

        private static List<Room> _rooms = new();
        private static List<Client> _awaitingClients = new(); 

        public RoomHub(TurnServerClientFactory turnServerClientFactory)
        {
            _turnServerClientFactory = turnServerClientFactory;
        }

        public async Task<Result<Unit>> JoinRoom(string roomName, string userName)
        {
            if (_rooms.Any(room => room.GroupName == roomName && room.Clients.Any(Client => Client.UserName == userName)) ||
                _awaitingClients.Any(client => client.RoomName == roomName && client.UserName == userName))
                return Result<Unit>.Error(new string[] { $"User {userName} has already joined to room {roomName}" });

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
                await Clients.Caller.OnRoomStarted(room.GroupName, room.IceServers);
                await Clients.GroupExcept(roomName, Context.ConnectionId).OnPeerJoined(roomName, userName);
            }
            return Result<Unit>.Success(Unit.Default);
        }

        public async Task<Result<Unit>> LeaveRoom(string roomName, string userName)
        {
            var client = new Client
            {
                ConnectionId = Context.ConnectionId,
                RoomName = roomName,
                UserName = userName
            };
            if (_rooms.Any(room => room.GroupName == roomName && room.Clients.Any(Client => Client.UserName == userName)))
                _rooms.Find(room => room.GroupName == roomName).Clients.ToList().Remove(client);
            else if (_awaitingClients.Any(client => client.RoomName == roomName && client.UserName == userName))
                _awaitingClients.Remove(client);
            else
                return Result<Unit>.Error(new string[] { $"User {userName} not found in room {roomName}" });

            await Clients.GroupExcept(roomName, Context.ConnectionId).OnPeerLeft(roomName, userName);
            return Result<Unit>.Success(Unit.Default);

        }


        public async Task<Result<Unit>> StartRoom(string roomName, string userName, TurnServer turnServer)
        {
            if (_rooms.Any(room => room.GroupName == roomName))
                return Result<Unit>.Error(new string[] { $"Room:{roomName} is in use" });

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
                await Clients.Group(roomName).OnRoomStarted(room.GroupName, room.IceServers);

                var excepts = new List<string>();
                foreach (var client in newRoomClients)
                {
                    excepts.Add(client.ConnectionId);
                    if (excepts.Count < newRoomClients.Count())
                        await Clients.GroupExcept(roomName, excepts).OnPeerJoined(client.RoomName, client.UserName);
                }

                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public async Task<Result<Unit>> StopRoom(string roomName, string userName)
        {
            var room = _rooms.Find(room => room.GroupName == roomName);

            if (room is null)
                return Result<Unit>.Error(new string[] { $"{roomName} room not found" });

            if (room.InitiatiorUserName != userName)
                return Result<Unit>.Error(new string[] { $"User {userName} has no authority to stop room {roomName}" });

            var excepts = new List<string>();
            foreach (var client in room.Clients)
            {
                excepts.Add(client.ConnectionId);
                if (excepts.Count < room.Clients.Count())
                    await Clients.GroupExcept(roomName, excepts).OnPeerLeft(client.RoomName, client.UserName);
            }
            await Clients.Group(roomName).OnRoomStopped(room.GroupName);
            _rooms.Remove(room);

            return Result<Unit>.Success(Unit.Default);
        }

        public async Task<Result<Unit>> SdpOffer(string roomName, string pairUserName, string sdp)
        {
            var room = _rooms.Find(room => room.GroupName == roomName);

            if (room is null)
                return Result<Unit>.Error(new string[] { $"{roomName} room not found" });

            var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
            var pairConnectionId = room.Clients.Single(client => client.UserName == pairUserName).ConnectionId; 

            await Clients.Client(pairConnectionId).OnPeerSdpOffered(roomName, userName, sdp);
            return Result<Unit>.Success(Unit.Default);
        }

        public async Task<Result<Unit>> SdpAnswer(string roomName, string pairUserName, string sdp)
        {
            var room = _rooms.Find(room => room.GroupName == roomName);

            if (room is null)
                return Result<Unit>.Error(new string[] { $"{roomName} room not found" });

            var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
            var pairConnectionId = room.Clients.Single(client => client.UserName == pairUserName).ConnectionId;

            await Clients.Client(pairConnectionId).OnPeerSdpAnswered(roomName, userName, sdp);
            return Result<Unit>.Success(Unit.Default);
        }
    }
}
