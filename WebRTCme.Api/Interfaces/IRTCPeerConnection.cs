using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Models;

namespace WebRTCme
{
    public interface IRTCPeerConnection : IAsyncDisposable
    {
        ValueTask<IAsyncDisposable> OnIceCandidate(Func<RTCPeerConnectionIceEvent, ValueTask> callback); 
    }
}
