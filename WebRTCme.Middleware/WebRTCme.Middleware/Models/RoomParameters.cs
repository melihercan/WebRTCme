using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class RoomParameters
    {
        public bool IsJoin { get; set; }

        public TurnServer TurnServer { get; set; }

        public string RoomId { get; set; }

        public string UserId { get; set; }
    }
}
