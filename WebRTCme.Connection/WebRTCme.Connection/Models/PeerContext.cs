using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public class PeerContext
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public IRTCPeerConnection PeerConnection { get; init; }
        public bool IsInitiator { get; init; }
    }
}
