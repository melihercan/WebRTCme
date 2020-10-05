using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRrtc.Android;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public IRTCPeerConnection CreateRTCPeerConnection(IJSRuntime jsRuntime) => new RTCPeerConnection();

        public Task<object> GetUserMedia(IJSRuntime jsRuntime)
        {
            throw new NotImplementedException();
        }

        //        public void Initialize(IServiceProvider serviceProvider)
        //      {
        //        throw new NotImplementedException();
        //  }
    }
}
