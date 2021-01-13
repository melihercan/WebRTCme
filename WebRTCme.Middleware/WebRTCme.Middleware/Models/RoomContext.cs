using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware;

namespace WebRtcMeMiddleware
{
    public class RoomContext
    {
        public RoomState RoomState { get; set; }
        public RoomRequestParameters RoomRequestParameters { get; set; }
    }
}
