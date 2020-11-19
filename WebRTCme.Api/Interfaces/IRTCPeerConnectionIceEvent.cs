using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCPeerConnectionIceEvent
    {
        Task<IRTCIceCandidate> Candidate { get; }
        
        ////string Url { get; }
    }
}
