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
    public class RoomHub : Hub<ISignallingServerCallbacks>, ISignallingServerClient
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

        public Task<Result<string[]>> GetTurnServerNamesAsync() =>
            Task.FromResult(Result<string[]>.Success(Enum.GetNames(typeof(TurnServer))));

        public async Task<Result<RTCIceServer[]>> GetIceServersAsync(string turnServerName)
        {
            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);
                return Result<RTCIceServer[]>.Success(await GetTurnServerClient(turnServer).GetIceServersAsync());

            }
            catch (Exception ex)
            {
                return Result<RTCIceServer[]>.Error(new string[] { ex.Message });
            }
        }


        public async Task<Result<Unit>> ReserveRoomAsync(string turnServerName, string roomName, string adminUserName, 
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

        public Task<Result<Unit>> FreeRoomAsync(string turnServerName, string roomName, string adminUserName)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Unit>> AddParticipantAsync(string turnServerName, string roomName, string participantUserName)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Unit>> RemoveParticipantAsync(string turnServerName, string roomName, string participantUserName)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<Unit>> JoinRoomAsync(string turnServerName, string roomName, string userName)
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
                                await Clients.GroupExcept(room.GroupName, excepts)
                                    .OnPeerJoinedAsync(turnServer.ToString(), client.RoomName, client.UserName); 
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
                        await Clients.GroupExcept(room.GroupName, Context.ConnectionId)
                            .OnPeerJoinedAsync(turnServer.ToString(), roomName, userName);
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
                    var groupName = $"{turnServer}.{roomName}";
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

        public async Task<Result<Unit>> LeaveRoomAsync(string turnServerName, string roomName, string userName)
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
                    {
                        if (room.IsReserved)
                        {
                            if (userName.Equals(room.AdminUserName, StringComparison.OrdinalIgnoreCase))
                            {
                                // Admin left. Terminate all connections.
                                //// TODO: COmplete
                            }
                            else
                            {
                                //// TODO: COmplete
                            }
                        }
                        else
                        {
                            // Non-reserved room peer left.
                            var client = room.Clients.Single(client => client.UserName.Equals(
                                userName, StringComparison.OrdinalIgnoreCase));
                            var groupName = room.GroupName;
                            var connectionId = client.ConnectionId;
                            room.Clients.Remove(client);
                            if (room.Clients.Count == 0)
                                server.Rooms.Remove(room);
                            if (server.Rooms.Count == 0)
                                Servers.Remove(server);
                            // Notify others.
                            await Clients.GroupExcept(groupName, connectionId)
                                .OnPeerLeftAsync(turnServer.ToString(), roomName, userName);
                            await Groups.RemoveFromGroupAsync(connectionId, groupName);
                        }
                    }
                    else
                    {
                        throw new Exception($"TURN Server:{turnServer} Room:{roomName} User:{userName} no user exist");
                    }
                }
                else
                {
                    // No room.
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} User:{userName} no room exist");
                }
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public async Task<Result<Unit>> OfferSdpAsync(string turnServerName, string roomName, string pairUserName, 
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

                await Clients.Client(pairConnectionId).OnPeerSdpOfferedAsync(turnServer.ToString(), roomName, userName, sdp);
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public async Task<Result<Unit>> AnswerSdpAsync(string turnServerName, string roomName, string pairUserName, 
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

                await Clients.Client(pairConnectionId).OnPeerSdpAnsweredAsync(turnServer.ToString(), roomName, userName, 
                    sdp);
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }
        }

        public async Task<Result<Unit>> IceCandidateAsync(string turnServerName, string roomName, string pairUserName, 
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

                await Clients.Client(pairConnectionId).OnPeerIceCandidateAsync(turnServer.ToString(), roomName, userName, 
                    ice);
                return Result<Unit>.Success(Unit.Default);
            }
            catch (Exception ex)
            {
                return Result<Unit>.Error(new string[] { ex.Message });
            }

        }

        public async ValueTask DisposeAsync()
        {
            //throw new NotImplementedException();
        }

    }
}
