using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public class UserContext
    {
        public ConnectionType ConnectionType { get; set; }

        public Guid Id { get; init; }

        public string Name { get; set; }

        public string Room { get; init; }

        public IMediaStream LocalStream { get; set; }

        // if null, no datachannel connection will be established.
        public string DataChannelName { get; set; }

    }
}
