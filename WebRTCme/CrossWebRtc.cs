using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public static class CrossWebRtc 
    {
        public static Task<IWebRtc> Instance => _webRtc.Value;


        private static Lazy<Task<IWebRtc>> _webRtc = new Lazy<Task<IWebRtc>>(() => CreateWebRtcAsync());


        private static Task<IWebRtc> CreateWebRtcAsync() => WebRtc.CreateAsync();

    }
}
