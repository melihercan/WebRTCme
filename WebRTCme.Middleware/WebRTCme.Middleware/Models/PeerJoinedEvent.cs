using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class PeerJoinedEvent
    {
        public string PeerUserName { get; init; }
        public IMediaStream MediaStream { get; init; }

    }
}
