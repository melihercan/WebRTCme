using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class RoomEvent
    {
        public RoomEventCode Code { get; init; }

        public string RoomName { get; init; }

        public string PeerUserName { get; init; }

        public IMediaStream MediaStream { get; init; }
    }
}
