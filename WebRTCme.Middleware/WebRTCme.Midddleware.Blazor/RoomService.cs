using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Blazor;
using WebRTCme.SignallingServerClient;

namespace WebRtcMiddlewareBlazor
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

        private RoomService()
        {

        }

        public Task CreateRoomAsync(RoomParameters roomParameters)
        {
            throw new NotImplementedException();
        }

        public Task JoinRoomAsync(RoomParameters roomParameters)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(_signallingServerClient.CleanupAsync());
        }

    }
}
