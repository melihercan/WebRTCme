using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;
using WebRTCme.SignallingServerClient;

namespace WebRtcMeMiddleware
{
    internal class RoomService : IRoomService
    {
        private ISignallingServerClient _signallingServerClient;
        private IJSRuntime _jsRuntime;

        static public async Task<IRoomService> CreateAsync(string signallingServerBaseUrl, IJSRuntime jsRuntime = null)
        {
            var self = new RoomService();
            self._signallingServerClient = SignallingServerClientFactory.Create(signallingServerBaseUrl);
            await self._signallingServerClient.InitializeAsync();
            self._jsRuntime = jsRuntime;
            return self;
        }

        private RoomService() { }

        public async Task ConnectRoomAsync(RoomParameters roomParameters)
        {
            var iceServers = await (roomParameters.IsJoin ?
                _signallingServerClient.JoinRoomAsync(roomParameters.TurnServer, roomParameters.RoomId, roomParameters.UserId) :
                _signallingServerClient.CreateRoomAsync(roomParameters.TurnServer, roomParameters.RoomId, roomParameters.UserId));

            var configuration = new RTCConfiguration
            {
                IceServers = iceServers,
                PeerIdentity = roomParameters.RoomId
            };
            var peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(_signallingServerClient.CleanupAsync());
        }

    }
}
