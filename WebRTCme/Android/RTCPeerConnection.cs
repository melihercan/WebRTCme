using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRrtc.Android
{
    internal class RTCPeerConnection : IRTCPeerConnection
    {
        public RTCPeerConnection()
        {
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
