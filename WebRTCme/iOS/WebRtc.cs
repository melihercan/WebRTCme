using System;
using System.Collections.Generic;
using System.Text;
using WebRtc.iOS;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public IRTCPeerConnection CreateRTCPeerConnection() => new RTCPeerConnection();
    }
}
