using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRrtc.Android
{
    internal class RTCPeerConnection : IRTCPeerConnection
    {
        public RTCPeerConnection()
        {
        }

        public void Do()
        {
            Console.WriteLine("I am Android");
        }
    }
}
