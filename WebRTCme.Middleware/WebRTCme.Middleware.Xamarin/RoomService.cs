using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Xamarin;
using WebRTCme.SignallingServerClient;

namespace WebRtcMiddlewareXamarin
{
    internal class RoomService : IRoomService
    {
        private ISignallingServerClient _signallingServerClient;

        static public async Task<IRoomService> CreateAsync()
        {
            var self = new RoomService();
            self._signallingServerClient = SignallingServerClientFactory.Create(WebRtcMiddleware.SignallingServerBaseUrl);
            await self._signallingServerClient.InitializeAsync();
            return self;
        }

        private RoomService() { }

        public async Task HandleRoomAsync(RoomParameters roomParameters)
        {
            var iceServers = await (roomParameters.IsJoin ?
                _signallingServerClient.JoinRoomAsync(roomParameters.TurnServer, roomParameters.RoomId, roomParameters.UserId) :
                _signallingServerClient.CreateRoomAsync(roomParameters.TurnServer, roomParameters.RoomId, roomParameters.UserId));
        }


        public ValueTask DisposeAsync()
        {
            return new ValueTask(_signallingServerClient.CleanupAsync());
        }
    }
}
