using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public enum RTCPeerConnectionState
    {
        New,
        Connecting,
        Connected,
        Disconnected,
        Failed,
        Closed
    }
}
