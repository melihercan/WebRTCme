using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        
        public Task<IWindow> Window()
        {
            //Org.Webrtc.ICameraEnumerator
            throw new NotImplementedException();
        }

        public Task<IWindow> Window(IJSRuntime jsRuntime)
        {
            throw new NotImplementedException();
        }
    }
}
