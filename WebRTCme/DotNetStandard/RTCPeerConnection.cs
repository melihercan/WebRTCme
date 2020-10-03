using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRTCme
{
    internal class RTCPeerConnection : IRTCPeerConnection
    {
        public RTCPeerConnection()
        {
        }

        public void Do()
        {
            Console.WriteLine("I am DotNetStandard");
        }
    }
}
