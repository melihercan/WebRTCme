using System;
using System.Collections.Generic;
using System.Text;
using WebRrtc.Android;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public IRTCPeerConnection CreateRTCPeerConnection() => new RTCPeerConnection();
    }
}
