using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class RoomRequestParameters
    {
        public bool IsInitiator { get; set; }

        public TurnServer TurnServer { get; set; }

        public string RoomId { get; set; }

        public string UserId { get; set; }

        public IMediaStream LocalStream { get; set; }
    }
}
