using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using WebRTCme.Connection.Signaling.Server.TurnServerProxies;
using WebRTCme.Connection.Signaling.Server.Enums;
using WebRTCme.Connection.Signaling.Server.Models;
using Utilme;

namespace WebRTCme.Connection.Signaling.Server.Hubs
{
    public class RoomHub : Hub<ISignalingServerNotify>, ISignalingServerApi
    {
        readonly ILogger<RoomHub> _logger;
        readonly ITurnServerProxy _turnServer;

        // Must be static as SignalR Hub crates a new context for each call.
        static Models.Server _server = new();

        // Currently these events are not used. Hence appended empty { add { } remove { } }.
        public event ISignalingServerNotify.PeerJoinedDelegateAsync PeerJoinedEventAsync;// { add { } remove { } }
        public event ISignalingServerNotify.PeerLeftDelegateAsync PeerLeftEventAsync;// { add { } remove { } }
        public event ISignalingServerNotify.PeerSdpAsyncDelegateAsync PeerSdpEventAsync;// { add { } remove { } }
        public event ISignalingServerNotify.PeerIceAsyncDelegateAsync PeerIceEventAsync;// { add { } remove { } }
        public event ISignalingServerNotify.PeerMediaAsyncDelegateAsync PeerMediaEventAsync;// { add { } remove { } }

        public RoomHub(TurnServerProxyFactory turnServerProxyFactory, ILogger<RoomHub> logger)
        {
            _logger = logger;
            _turnServer = turnServerProxyFactory.Create(TurnServer.StunOnly);
        }

        public async Task<Result<RTCIceServer[]>> GetIceServersAsync()
        {
            try
            {
//                var turnServer = GetTurnServerFromName(turnServerName);
                return Result<RTCIceServer[]>.Ok(await _turnServer.GetIceServersAsync());
            }
            catch(Exception)
            {
                return Result<RTCIceServer[]>.Error("IceServers not found");
            }
        }

        public async Task<Result<Unit>> JoinAsync(Guid id, string name, string room)
        {
            try
            {
                bool notifyOthers = true;

                _logger.LogInformation($"######## JoinAsync - id:{id} name:{name} room:{room}");

                var client = _server.Rooms.SelectMany(r => r.Clients).SingleOrDefault(c => c.Id.Equals(id));
                if (client is not null)
                    throw new Exception($"Room:{room} User:{name} Id:{id} has already joined");

                _server.IceServers ??= await _turnServer.GetIceServersAsync();

                var room_ = _server.Rooms.SingleOrDefault(r => r.RoomName.Equals(room, StringComparison.OrdinalIgnoreCase));
                if (room_ is null)
                {
                    room_ = new Room
                    {
                        RoomName = room,
                        GroupName = room,
                    };
                    _server.Rooms.Add(room_);
                    notifyOthers = false;
                }
                var groupName = room_.GroupName;
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                room_.Clients.Add(new Client
                {
                    ConnectionId = Context.ConnectionId,
                    Id = id,
                    RoomName = room,
                    UserName = name
                });

                if (notifyOthers)
                    await Clients.GroupExcept(groupName, Context.ConnectionId).OnPeerJoinedAsync(id, name);

                return Result<Unit>.Ok(Unit.Default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result<Unit>.Error(ex.Message);
            }
        }

        public async Task<Result<Unit>> LeaveAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"######## LeaveAsync - id:{id}");

                var client = _server.Rooms.SelectMany(r => r.Clients).SingleOrDefault(c => c.Id.Equals(id));
                if (client is null)
                    throw new Exception($"id:{id} no user found");
                
                var room = _server.Rooms.Single(r => 
                    r.RoomName.Equals(client.RoomName, StringComparison.OrdinalIgnoreCase));
                var groupName = room.GroupName;

                room.Clients.Remove(client);
                if (room.Clients.Count == 0)
                    _server.Rooms.Remove(room);

                // Notify others.
                await Clients.GroupExcept(groupName, client.ConnectionId).OnPeerLeftAsync(id);
                await Groups.RemoveFromGroupAsync(client.ConnectionId, groupName);

                return Result<Unit>.Ok(Unit.Default);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result<Unit>.Error(ex.Message);
            }
        }

        public async Task<Result<Unit>> SdpAsync(Guid peerId, string sdp)
        {
            try
            {
                var selfClient = _server.Rooms
                    .SelectMany(r => r.Clients)
                    .Single(c => c.ConnectionId == Context.ConnectionId);
                var room = _server.Rooms.Single(r =>
                    r.RoomName.Equals(selfClient.RoomName, StringComparison.OrdinalIgnoreCase));

                var peerClient = _server.Rooms.SelectMany(r => r.Clients).SingleOrDefault(c => c.Id.Equals(peerId));
                if (peerClient is null)
                    throw new Exception($"peerId:{peerId} no peer found");

                await Clients.Client(peerClient.ConnectionId).OnPeerSdpAsync(selfClient.Id, selfClient.UserName, sdp);
                return Result<Unit>.Ok(Unit.Default);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result<Unit>.Error(ex.Message);
            }
        }

        public async Task<Result<Unit>> IceAsync(Guid peerId, string ice)
        {
            try
            {
                var selfClient = _server.Rooms
                    .SelectMany(r => r.Clients)
                    .Single(c => c.ConnectionId == Context.ConnectionId);
                var room = _server.Rooms.Single(r =>
                    r.RoomName.Equals(selfClient.RoomName, StringComparison.OrdinalIgnoreCase));

                var peerClient = _server.Rooms.SelectMany(r => r.Clients).SingleOrDefault(c => c.Id.Equals(peerId));
                if (peerClient is null)
                    throw new Exception($"peerId:{peerId} no peer found");

                await Clients.Client(peerClient.ConnectionId).OnPeerIceAsync(selfClient.Id, ice);
                return Result<Unit>.Ok(Unit.Default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result<Unit>.Error(ex.Message);
            }
        }

        public async Task<Result<Unit>> MediaAsync(Guid id, bool videoMuted, bool audioMuted, bool speaking)
        {
            try
            {
                _logger.LogInformation($"######## LeaveAsync - id:{id}");

                var client = _server.Rooms.SelectMany(r => r.Clients).SingleOrDefault(c => c.Id.Equals(id));
                if (client is null)
                    throw new Exception($"id:{id} no user found");

                var room = _server.Rooms.Single(r =>
                    r.RoomName.Equals(client.RoomName, StringComparison.OrdinalIgnoreCase));
                var groupName = room.GroupName;

                // Notify others.
                await Clients.GroupExcept(groupName, client.ConnectionId)
                    .OnPeerMediaAsync(id, videoMuted, audioMuted, speaking);

                return Result<Unit>.Ok(Unit.Default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result<Unit>.Error(ex.Message);
            }

        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

    }
}
