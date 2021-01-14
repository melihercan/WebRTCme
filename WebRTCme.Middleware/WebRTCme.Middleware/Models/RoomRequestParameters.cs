using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class RoomRequestParameters
    {
        public bool IsInitiator { get; set; }

        public TurnServer TurnServer { get; set; }

        public string RoomName { get; set; }

        public string UserName { get; set; }

        public IMediaStream LocalStream { get; set; }
    }
}
