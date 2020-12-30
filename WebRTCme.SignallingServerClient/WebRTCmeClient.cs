using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    internal class WebRTCmeClient : ISignallingServerClient
    {
        public Task CleanupAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RTCIceServer[]> CreateRoomAsync(string roomId, string clientId)
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RTCIceServer[]> JoinRoomAsync(string roomId, string clientId)
        {
            throw new NotImplementedException();
        }
    }
}
