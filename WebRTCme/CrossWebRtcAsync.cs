using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public static class CrossWebRtcAsync 
    {
        public static Task<IWebRtc> CurrentAsync => _webRtc.Value;

        private static Lazy<Task<IWebRtc>> _webRtc = new Lazy<Task<IWebRtc>>(() => CreateWebRtcAsync());

        private static Task<IWebRtc> CreateWebRtcAsync() => null;// WebRtcAsync.CreateAsync();
    }
}
