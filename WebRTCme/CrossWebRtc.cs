using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public static class CrossWebRtc
    {
        private static Lazy<IWebRtc> _webRtc = new Lazy<IWebRtc>(() => CreateWebRtc());

        public static IWebRtc Current => _webRtc.Value;

        private static IWebRtc CreateWebRtc() => new WebRtc();
    }
}
