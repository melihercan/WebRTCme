using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public enum RTCIceConnectionState
    {
        New,
        Checking,
        Connected,
        Completed,
        Failed,
        Disconnected,
        Closed
    }
}
