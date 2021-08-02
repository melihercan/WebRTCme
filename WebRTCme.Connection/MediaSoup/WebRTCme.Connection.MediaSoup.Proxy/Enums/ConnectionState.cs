using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public enum ConnectionState
    {
        New,
        Connecting,
        Connected,
        Failed,
        Disconnected,
        Closed
    }
}
