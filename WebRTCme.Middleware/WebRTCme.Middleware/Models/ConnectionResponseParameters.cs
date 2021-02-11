using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class ConnectionResponseParameters
    {
        public string TurnServerName { get; init; }
        
        public string RoomName { get; init; }

        public string PeerUserName { get; init; }

        public IMediaStream MediaStream { get; init; }

        public IRTCDataChannel DataChannel { get; init; }
    }
}
