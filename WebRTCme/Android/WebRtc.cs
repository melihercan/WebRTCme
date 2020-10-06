using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRrtc.Android;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public Task<IWindow> NewWindow()
        {
            throw new NotImplementedException();
        }
    }
}
