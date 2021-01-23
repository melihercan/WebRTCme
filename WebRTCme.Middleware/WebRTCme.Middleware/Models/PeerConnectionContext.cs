using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtcMeMiddleware.Models
{
    internal class PeerConnectionContext
    {
        public string PeerUserName { get; set; }
        
        public IRTCPeerConnection PeerConnection { get; set; }

        public bool IsInitiator { get; set; }
    }
}
