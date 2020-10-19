using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCPeerConnectionIceEvent
    {
        IRTCIceCandidate Candidate { get; set; }
    }
}
