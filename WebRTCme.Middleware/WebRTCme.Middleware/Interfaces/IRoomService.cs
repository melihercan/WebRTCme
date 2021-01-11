using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IRoomService : IAsyncDisposable
    {
        Task<IMediaStream> ConnectRoomAsync(RoomRequestParameters roomRequestParameters);
    }
}
