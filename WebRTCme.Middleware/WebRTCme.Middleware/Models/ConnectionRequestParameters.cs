using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class ConnectionRequestParameters
    {
        public string TurnServerName { get; set; }

        public string RoomName { get; set; }

        public string UserName { get; set; }

        // If null, no streaming will take place.
        public IMediaStream LocalStream { get; set; }

        // if null, no datachannel connection will be established.
        public string DataChannelName { get; set; }
    }
}
