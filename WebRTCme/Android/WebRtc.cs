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
        public Task<IWindow> Window()
        {
            throw new NotImplementedException();
        }

        public Task<IWindow> Window(IJSRuntime jsRuntime)
        {
            throw new NotImplementedException();
        }
    }
}
