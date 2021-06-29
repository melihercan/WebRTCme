using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using WebRTCme.SignallingServer.Enums;
using WebRTCme.SignallingServer.Models;
using WebRTCme.SignallingServer.TurnServerService;
using WebRTCme.SignallingServerProxy;

namespace WebRTCme.SignallingServer.Hubs
{
    public class RoomHub : Hub<ISignallingServerCallbacks>, ISignallingServerProxy
    {
        readonly TurnServerProxyFactory _turnServerProxyFactory;
        readonly ILogger<RoomHub> _logger;

        static List<Server> Servers = new();
        static Dictionary<TurnServer, ITurnServerProxy> TurnServerProxies = new();

        // Currently these events are not used. Hence appended empty { add { } remove { } }.
        public event ISignallingServerProxy.JoinedOrLeftCallbackHandler OnPeerJoinedAsyncEvent { add { } remove { } }
        public event ISignallingServerProxy.JoinedOrLeftCallbackHandler OnPeerLeftAsyncEvent { add { } remove { } }
        public event ISignallingServerProxy.SdpOrIceCallbackHandler OnPeerSdpAsyncEvent { add { } remove { } }
        public event ISignallingServerProxy.SdpOrIceCallbackHandler OnPeerIceAsyncEvent { add { } remove { } }

        public RoomHub(TurnServerProxyFactory turnServerProxyFactory, ILogger<RoomHub> logger)
        { 
            _turnServerProxyFactory = turnServerProxyFactory;
            _logger = logger;
        }

        ITurnServerProxy GetTurnServerClient(TurnServer turnServer)
        {
            if (!TurnServerProxies.ContainsKey(turnServer))
                TurnServerProxies.Add(turnServer, _turnServerProxyFactory.Create(turnServer));
            return TurnServerProxies[turnServer];
        }

        TurnServer GetTurnServerFromName(string turnServerName) =>
            (TurnServer)Enum.Parse(typeof(TurnServer), turnServerName, true);

        public Task<(SignallingServerResult, string[])> GetTurnServerNamesAsync() =>
            Task.FromResult((SignallingServerResult.Ok, Enum.GetNames(typeof(TurnServer))));

        public async Task<(SignallingServerResult, RTCIceServer[])> GetIceServersAsync(string turnServerName)
        {
            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);
                return (SignallingServerResult.Ok, await GetTurnServerClient(turnServer).GetIceServersAsync());
            }
            catch(Exception)
            {
                return (SignallingServerResult.IceServersNotFound, null);
            }
        }

        public async Task<SignallingServerResult> JoinRoomAsync(string turnServerName, string roomName, 
            string userName)
        {
            SignallingServerResult err = SignallingServerResult.Error;

            try
            {
                _logger.LogInformation(
                    $"######## JoinRoomAsync - turn:{turnServerName} room:{roomName}  userName:{userName}");

                var turnServer = GetTurnServerFromName(turnServerName);

                if (Servers.Any(server => server.TurnServer == turnServer &&
                    server.Rooms.Any(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase))))
                {
                    // Room exists.
                    var server = Servers.Single(server => server.TurnServer == turnServer);
                    var room = server.Rooms
                        .Single(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));
                    if (room.Clients.Any(client => client.UserName.Equals(userName,
                        StringComparison.OrdinalIgnoreCase)))
                    {
                        err = SignallingServerResult.UserNameIsInUse;
                        throw new Exception($"TURN Server:{turnServer} Room:{roomName} User:{userName} " +
                            $"has already joined");
                    }

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
                else
                {
                    // First user.
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

                return SignallingServerResult.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return (err);
            }
        }

        public async Task<SignallingServerResult> LeaveRoomAsync(string turnServerName, string roomName, 
            string userName)
        {
            SignallingServerResult err = SignallingServerResult.Error;

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
                    if (room.Clients.Any(client => client.UserName.Equals(userName,
                        StringComparison.OrdinalIgnoreCase)))
                    {
                        // Peer left.
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
                    else
                    {
                        err = SignallingServerResult.UserNotFound;
                        throw new Exception($"TURN Server:{turnServer} Room:{roomName} User:{userName} no user exist");
                    }
                }
                else
                {
                    // No room.
                    err = SignallingServerResult.RoomNotFound;
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} User:{userName} no room exist");
                }

                return SignallingServerResult.Ok;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return (err);
            }
        }

        public async Task<SignallingServerResult> SdpAsync(string turnServerName, string roomName, 
            string peerUserName, string sdp)
        {
            SignallingServerResult err = SignallingServerResult.Error;

            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);
                var room = Servers
                    .Find(server => server.TurnServer == turnServer)
                    .Rooms.Find(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));
                if (room is null)
                {
                    err = SignallingServerResult.RoomNotFound;
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} is not found");
                }

                var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
                var peerConnectionId = room.Clients
                    .SingleOrDefault(client => client.UserName.Equals(peerUserName, StringComparison.OrdinalIgnoreCase))
                    .ConnectionId;
                if (peerConnectionId is null)
                {
                    err = SignallingServerResult.PeerNotFound;
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} PeerUserName:{peerUserName} no peer found");
                }

                await Clients.Client(peerConnectionId).OnPeerSdpAsync(turnServer.ToString(), roomName, userName, sdp);

                return SignallingServerResult.Ok;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return (err);
            }
        }

        public async Task<SignallingServerResult> IceCandidateAsync(string turnServerName, string roomName, 
            string peerUserName, string ice)
        {
            SignallingServerResult err = SignallingServerResult.Error;

            try
            {
                var turnServer = GetTurnServerFromName(turnServerName);
                var room = Servers
                    .Find(server => server.TurnServer == turnServer)
                    .Rooms.Find(room => room.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));
                if (room is null)
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} is not found");

                var userName = room.Clients.Single(client => client.ConnectionId == Context.ConnectionId).UserName;
                var peerConnectionId = room.Clients
                    .SingleOrDefault(client => client.UserName.Equals(peerUserName, StringComparison.OrdinalIgnoreCase))
                    .ConnectionId;
                if (peerConnectionId is null)
                {
                    err = SignallingServerResult.PeerNotFound;
                    throw new Exception($"TURN Server:{turnServer} Room:{roomName} PeerUserName:{peerUserName} no peer found");
                }

                await Clients.Client(peerConnectionId).OnPeerIceCandidateAsync(turnServer.ToString(), roomName, userName,
                    ice);

                return SignallingServerResult.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return (err);
            }
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

    }
}
