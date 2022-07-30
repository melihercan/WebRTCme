using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public static class CrossWebRtc 
    {
        public static IWebRtc Current => _webRtc.Value;

        private static readonly Lazy<IWebRtc> _webRtc = new Lazy<IWebRtc>(() => CreateWebRtc());                        

        private static IWebRtc CreateWebRtc() => WebRtc.Create();
    }
}
