using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.SignallingServerProxy
{
    public enum SignallingServerResult
    {
        Ok,
        Error,
        IceServersNotFound,
        RoomNotFound,
        UserNotFound,
        PeerNotFound,
        RoomIsInUse,
        UserNameIsInUse,
    }
}
