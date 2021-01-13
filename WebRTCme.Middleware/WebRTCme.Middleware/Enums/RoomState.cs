using System;
using System.Collections.Generic;
using System.Text;

namespace WebRtcMeMiddleware
{
    public enum RoomState
    {
        Idle,
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        Error
    }
}
