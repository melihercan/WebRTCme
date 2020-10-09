using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCPeerConnection : IRTCPeerConnection
    {
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public void Do()
        {
            Console.WriteLine("I am iOS");
        }
    }
}
