using Ardalis.Result;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using WebRTCme.SignallingServer.Enums;
using WebRTCme.SignallingServer.Models;
using WebRTCme.SignallingServer.TurnServerService;
using WebRTCme.SignallingServerClient;

namespace WebRTCme.SignallingServer.Hubs
{
    public class RoomHub : Hub<ISignallingServerCallbacks>, ISignallingServerRequests
    {
        private readonly TurnServerClientFactory _turnServerClientFactory;

        private static List<Server> Servers = new();

//        private static List<Room> _rooms = new();
  //      private static List<Client> AwaitingClients = new();
        private static Dictionary<TurnServer, ITurnServerClient> TurnServerClients = new();

        public RoomHub(TurnServerClientFactory turnServerClientFactory)
        {
            _turnServerClientFactory = turnServerClientFactory;
        }

        private ITurnServerClient GetTurnServerClient(TurnServer turnServer)
        {
            if (!TurnServerClients.ContainsKey(turnServer))
                TurnServerClients.Add(turnServer, _turnServerClientFactory.Create(turnServer));
            return TurnServerClients[turnServer];
        }

        private TurnServer GetTurnServerFromName(string turnServerName) =>
            (TurnServer)Enum.Parse(typeof(TurnServer), turnServerName, true);

        public Task<Result<string[]>> GetTurnServerNames() =>
            Task.FromResult(Result<string[]>.Success(Enum.GetNames(typeof(TurnServer))));

        public async Task<Result<Unit>> ReserveRoom(string turnServerName, string roomName, string adminUserName, 
            string[] participantUserNames)
        {
            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);

                if (Servers.Any(server => server.TurnServer == turnServer &&
                    server.Rooms.Any(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase))))
                        throw new Exception($"TURN Server:{turnServer} Room:{roomName} is in use");

                var iceServers = await GetTurnServerClient(turnServer).GetIceServersAsync();
                var rooms = new List<Room>();
                rooms.Add(new Room
                {
                    RoomName = roomName,
                    GroupName = $"{turnServer}.{roomName}",
                    IsReserved = true,
                    AdminUserName = adminUserName,
                    Clients = participantUserNames.Select(name => new Client 
                    { 
                        // ConnectionId = will be set in 'JoinRoom' call
                        TurnServer = turnServer,
                        RoomName = roomName,
                        UserName = name
                    }).ToList()
                });
                Servers.Add(new Server
                {
                    TurnServer = turnServer,
                    IceServers = iceServers,
                    Rooms = rooms
                });
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public Task<Result<Unit>> FreeRoom(string turnServerName, string roomName)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Unit>> AddParticipant(string turnServerName, string roomName, string participantUserName)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Unit>> RemoveParticipant(string turnServerName, string roomName, string participantUserName)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<Unit>> JoinRoom(string turnServerName, string roomName, string userName)
        {
            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);

                if (Servers.Any(server => server.TurnServer == turnServer &&
                    server.Rooms.Any(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase))))
                {
                    // Room exists.
                    var server = Servers.Single(server => server.TurnServer == turnServer);
                    var room = server.Rooms
                        .Single(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));
                    if (room.Clients.Any(client => client.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)))
                        throw new Exception($"TURN Server:{turnServer} Room:{roomName} User:{userName} has already joined");

                    if (room.IsReserved)
                    {
                        // Reserved room. Update client's ConnectionId.
                        room.Clients
                            .Single(client => client.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                            .ConnectionId = Context.ConnectionId;

                        if (userName.Equals(room.AdminUserName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Admin joined. Take action.
                            var excepts = new List<string>();
                            foreach (var client in room.Clients)
                            {
                                excepts.Add(client.ConnectionId);
                                await Clients.GroupExcept(roomName, excepts)
                                    .OnPeerJoined(turnServer.ToString(), client.RoomName, client.UserName, 
                                        server.IceServers);
                            }
                        }
                        //else
                        //{
                        // Participant joined.
                        //}

                    }
                    else
                    {
                        // Non-reserved room.
                        room.Clients.Add(new Client
                        {
                            ConnectionId = Context.ConnectionId,
                            TurnServer = turnServer,
                            RoomName = roomName,
                            UserName = userName
                        });
                        await Groups.AddToGroupAsync(Context.ConnectionId, room.GroupName);
                        // Notify others.
                        await Clients.GroupExcept(roomName, Context.ConnectionId)
                            .OnPeerJoined(turnServer.ToString(), roomName, userName, server.IceServers);
                    }
                }
                else
                {
                    // First user of non-reserved room.
                    var server = Servers.Find(server => server.TurnServer == turnServer);
                    if (server is null)
                    {
                        server = new Server
                        {
                            TurnServer = turnServer,
                            IceServers = await GetTurnServerClient(turnServer).GetIceServersAsync(),
                            Rooms = new List<Room>()
                        };
                        Servers.Add(server);
                    }
                    var groupName = "{turnServer}.{roomName}";
                    var room = new Room
                    {
                        RoomName = roomName,
                        GroupName = groupName,
                        IsReserved = false,
                        Clients = new List<Client>()
                    };
                    room.Clients.Add(new Client
                    {
                        ConnectionId = Context.ConnectionId,
                        TurnServer = turnServer,
                        RoomName = roomName,
                        UserName = userName
                    });
                    server.Rooms.Add(room);
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                }
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public async Task<Result<Unit>> OfferSdp(string turnServerName, string roomName, string pairUserName, 
            string sdp)
        {
            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);
                var room = Servers
                    .Find(server => server.TurnServer == turnServer)
                    .Rooms.Find(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));
                if (room is null)
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} is not found");

                var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
                var pairConnectionId = room.Clients
                    .Single(client => client.UserName.Equals(pairUserName, StringComparison.OrdinalIgnoreCase))
                    .ConnectionId;

                await Clients.Client(pairConnectionId).OnPeerSdpOffered(turnServer.ToString(), roomName, userName, sdp);
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public async Task<Result<Unit>> AnswerSdp(string turnServerName, string roomName, string pairUserName, 
            string sdp)
        {
            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);
                var room = Servers
                    .Find(server => server.TurnServer == turnServer)
                    .Rooms.Find(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));
                if (room is null)
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} is not found");

                var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
                var pairConnectionId = room.Clients
                    .Single(client => client.UserName.Equals(pairUserName, StringComparison.OrdinalIgnoreCase))
                    .ConnectionId;

                await Clients.Client(pairConnectionId).OnPeerSdpAnswered(turnServer.ToString(), roomName, userName, sdp);
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public async Task<Result<Unit>> IceCandidate(string turnServerName, string roomName, string pairUserName, 
            string ice)
        {
            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);
                var room = Servers
                    .Find(server => server.TurnServer == turnServer)
                    .Rooms.Find(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));
                if (room is null)
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} is not found");

                var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
                var pairConnectionId = room.Clients
                    .Single(client => client.UserName.Equals(pairUserName, StringComparison.OrdinalIgnoreCase))
                    .ConnectionId;

                await Clients.Client(pairConnectionId).OnPeerSdpOffered(turnServer.ToString(), roomName, userName, ice);
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }

        }


#if false
        public async Task<Result<Unit>> ReserveRoom(TurnServer turnServer, string roomName, string userName)
        {
            try
            {
                if (Servers.Any(server => server.Server == turnServer && 
                    server.Rooms.Any(room => room.RoomName == roomName)))// ||
                ////AwaitingClients.Any(client => client.TurnServer == turnServer && client.RoomName == roomName))
                        throw new Exception($"TURN Server:{turnServer} Room:{roomName} is in use");

                var iceServers = await GetTurnServerClient(turnServer).GetIceServersAsync();
                var rooms = new List<Room>();
                rooms.Add(new Room 
                { 
                    RoomName = roomName,
                    GroupName = $"{turnServer}.{roomName}",
                    IsReserved = true,
                    AdminUserName = userName,
                    Clients = new List<Client>()
                });
                Servers.Add(new Models.Server 
                { 
                    Server = turnServer,
                    IceServers = iceServers,
                    Rooms = rooms
                });
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public async Task<Result<Unit>> JoinRoom(TurnServer turnServer, string roomName, string userName)
        {
            try
            {
                if (Servers.Any(server => server.Server == turnServer &&
                    server.Rooms.Any(room => room.RoomName == roomName)))
                {
                    // Room already exists.
                    var room = Servers
                        .Single(server => server.Server == turnServer)
                        .Rooms.Single(room => room.RoomName == roomName);
                    if (room.Clients.Any(client => client.UserName == userName))
                        throw new Exception($"TURN Server:{turnServer} Room:{roomName} User:{userName} has already joined");

                    // Add user.
                    room.Clients.Add(new Client
                    {
                        ConnectionId = Context.ConnectionId,
                        TurnServer = turnServer,
                        RoomName = roomName,
                        UserName = userName
                    });
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

                    if (room.IsReserved && room.ReserverUserName == userName)
                    {
                        // Room is reserved and reserver joined, notify all parties about each other.
                        room.Clients.Select(client => Clients.GroupExcept(room.GroupName, client.ConnectionId)
                        .OnPeerJoined(roomName, userName, )
                    }
                }
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }


        public async Task<Result<Unit>> JoinRoom(string roomName, string userName)
        {
            if (_rooms.Any(room => room.GroupName == roomName && room.Clients.Any(client => client.UserName == userName)) ||
                AwaitingClients.Any(client => client.RoomName == roomName && client.UserName == userName))
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
                AwaitingClients.Add(client);
            else
            {
                // Room has been started. Add this client to room, send IceServer list and notify all other clients
                // in the group.
                room.Clients = room.Clients.Append(client);
                await Groups.AddToGroupAsync(client.ConnectionId, roomName);
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
            else if (AwaitingClients.Any(client => client.RoomName == roomName && client.UserName == userName))
            {
                await Groups.RemoveFromGroupAsync(client.ConnectionId, roomName);
                AwaitingClients.Remove(client);
            }
            else
                return Result<Unit>.Error(new string[] { $"User {userName} not found in room {roomName}" });

            await Clients.GroupExcept(roomName, Context.ConnectionId).OnPeerLeft(roomName, userName);
            return Result<Unit>.Success(Unit.Default);

        }


        public async Task<Result<Unit>> StartRoom(string roomName, string userName, TurnServer turnServer)
        {
            if (_rooms.Any(room => room.GroupName == roomName))
                return Result<Unit>.Error(new string[] { $"Room:{roomName} is in use" });

            var turnServerClient = _turnServerClientFactory.Create(turnServer);

            try
            {
                var iceServers = /*new RTCIceServer[] { };*/ await turnServerClient.GetIceServersAsync();
                var newRoomClients = AwaitingClients.Where(client => client.RoomName == roomName).ToList();
                AwaitingClients.RemoveAll(client => client.RoomName == roomName);
                var room = new Room
                {
                    GroupName = roomName,
                    IceServers = iceServers,
                    AdminUserName = userName,
                    Clients = newRoomClients
                };
                _rooms.Add(room);
                await Task.WhenAll(room.Clients.Select(client => Groups.AddToGroupAsync(client.ConnectionId, roomName)));
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

            if (room.AdminUserName != userName)
                return Result<Unit>.Error(new string[] { $"User {userName} has no authority to stop room {roomName}" });

            var excepts = new List<string>();
            foreach (var client in room.Clients)
            {
                excepts.Add(client.ConnectionId);
                if (excepts.Count < room.Clients.Count())
                    await Clients.GroupExcept(roomName, excepts).OnPeerLeft(client.RoomName, client.UserName);
            }
            await Clients.Group(roomName).OnRoomStopped(room.GroupName);
            await Task.WhenAll(room.Clients.Select(client => Groups.RemoveFromGroupAsync(client.ConnectionId, roomName)));
            _rooms.Remove(room);

            return Result<Unit>.Success(Unit.Default);
        }

        public async Task<Result<Unit>> OfferSdp(string roomName, string pairUserName, string sdp)
        {
            var room = _rooms.Find(room => room.GroupName == roomName);

            if (room is null)
                return Result<Unit>.Error(new string[] { $"{roomName} room not found" });

            var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
            var pairConnectionId = room.Clients.Single(client => client.UserName == pairUserName).ConnectionId; 

            await Clients.Client(pairConnectionId).OnPeerSdpOffered(roomName, userName, sdp);
            return Result<Unit>.Success(Unit.Default);
        }

        public async Task<Result<Unit>> AnswerSdp(string roomName, string pairUserName, string sdp)
        {
            var room = _rooms.Find(room => room.GroupName == roomName);

            if (room is null)
                return Result<Unit>.Error(new string[] { $"{roomName} room not found" });

            var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
            var pairConnectionId = room.Clients.Single(client => client.UserName == pairUserName).ConnectionId;

            await Clients.Client(pairConnectionId).OnPeerSdpAnswered(roomName, userName, sdp);
            return Result<Unit>.Success(Unit.Default);
        }

        public async Task<Result<Unit>> IceCandidate(string roomName, string pairUserName, string ice)
        {
            var room = _rooms.Find(room => room.GroupName == roomName);

            if (room is null)
                return Result<Unit>.Error(new string[] { $"{roomName} room not found" });

            var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
            var pairConnectionId = room.Clients.Single(client => client.UserName == pairUserName).ConnectionId;

            await Clients.Client(pairConnectionId).OnPeerIceCandidate(roomName, userName, ice);
            return Result<Unit>.Success(Unit.Default);
        }
#endif

    }
}
