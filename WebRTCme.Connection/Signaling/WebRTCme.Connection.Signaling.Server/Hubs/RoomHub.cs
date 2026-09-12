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

        // ISignalingServerApi requires these events, but the hub notifies its clients through
        // Hub<ISignalingServerNotify>.Clients instead, so it never raises them. Empty accessors
        // satisfy the interface without declaring backing fields nothing ever assigns.
        public event ISignalingServerNotify.PeerJoinedDelegateAsync PeerJoinedEventAsync { add { } remove { } }
        public event ISignalingServerNotify.PeerLeftDelegateAsync PeerLeftEventAsync { add { } remove { } }
        public event ISignalingServerNotify.PeerSdpAsyncDelegateAsync PeerSdpEventAsync { add { } remove { } }
        public event ISignalingServerNotify.PeerIceAsyncDelegateAsync PeerIceEventAsync { add { } remove { } }
        public event ISignalingServerNotify.PeerMediaAsyncDelegateAsync PeerMediaEventAsync { add { } remove { } }

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

        /// <summary>
        /// Treats a dropped transport as the client leaving.
        /// </summary>
        /// <remarks>
        /// There was no override here, so the only way out of a room was a client politely calling
        /// <see cref="LeaveAsync"/> first. Anything that skipped that - a reloaded browser tab, a
        /// killed app, a phone losing wifi - left the client in the room for the lifetime of the
        /// process, and left every other peer holding a peer connection to it.
        ///
        /// That is not a tidiness problem. Measured on 2026-09-12 by reloading a browser tab
        /// mid-call: the Android peer carried on encoding and sending a second full camera stream
        /// to the tab that had gone, 4763 frames and climbing, for a peer that no longer existed.
        /// Double the encode and double the uplink, indefinitely, with nothing on screen to say so.
        ///
        /// Two smaller consequences went with it. Rooms were never removed, because removal only
        /// happens when the last client leaves. And <see cref="JoinAsync"/> refuses an id that is
        /// already in a room, so a client reconnecting with the same id was turned away by its own
        /// ghost - which is the shape of bug that looks like "it works until you refresh".
        /// </remarks>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var client = _server.Rooms
                .SelectMany(room => room.Clients)
                .SingleOrDefault(candidate => candidate.ConnectionId == Context.ConnectionId);

            if (client is not null)
            {
                _logger.LogInformation(
                    $"######## OnDisconnected - id:{client.Id} name:{client.UserName} " +
                    $"room:{client.RoomName} reason:{exception?.Message ?? "closed"}");

                await RemoveClientAsync(client);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Takes a client out of its room and tells the others, however it came to be leaving.
        /// </summary>
        /// <remarks>
        /// Shared by <see cref="LeaveAsync"/> and <see cref="OnDisconnectedAsync"/> so the two
        /// cannot drift. They did not exist as two paths before; this is the second one arriving.
        /// </remarks>
        async Task RemoveClientAsync(Client client)
        {
            var room = _server.Rooms.SingleOrDefault(candidate =>
                candidate.RoomName.Equals(client.RoomName, StringComparison.OrdinalIgnoreCase));

            if (room is null)
                return;

            var groupName = room.GroupName;

            room.Clients.Remove(client);
            if (room.Clients.Count == 0)
                _server.Rooms.Remove(room);

            await Clients.GroupExcept(groupName, client.ConnectionId).OnPeerLeftAsync(client.Id);
            await Groups.RemoveFromGroupAsync(client.ConnectionId, groupName);
        }

        public async Task<Result<Unit>> LeaveAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"######## LeaveAsync - id:{id}");

                var client = _server.Rooms.SelectMany(r => r.Clients).SingleOrDefault(c => c.Id.Equals(id));
                if (client is null)
                    throw new Exception($"id:{id} no user found");

                await RemoveClientAsync(client);

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
                _logger.LogInformation($"######## MediaAsync - id:{id} videoMuted:{videoMuted} " +
                    $"audioMuted:{audioMuted} speaking:{speaking}");

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
