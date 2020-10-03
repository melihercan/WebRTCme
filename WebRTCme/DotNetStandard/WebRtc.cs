using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public IRTCPeerConnection CreateRTCPeerConnection() => new RTCPeerConnection();
    }
}
