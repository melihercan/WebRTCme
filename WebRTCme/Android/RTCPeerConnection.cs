using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Models;

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

        public ValueTask<IAsyncDisposable> OnIceCandidate(Func<RTCPeerConnectionIceEvent, ValueTask> callback)
        {
            throw new NotImplementedException();
        }
    }
}
