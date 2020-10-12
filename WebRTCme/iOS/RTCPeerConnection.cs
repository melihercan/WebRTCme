using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Models;

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

        public ValueTask<IAsyncDisposable> OnIceCandidate(Func<RTCPeerConnectionIceEvent, ValueTask> callback)
        {
            throw new NotImplementedException();
        }
    }
}
