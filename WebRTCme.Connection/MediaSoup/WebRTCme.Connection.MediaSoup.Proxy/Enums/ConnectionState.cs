using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public enum ConnectionState
    {
        Connecting,
        Connected,
        Failed,
        Disconnected,
        Closed
    }
}
