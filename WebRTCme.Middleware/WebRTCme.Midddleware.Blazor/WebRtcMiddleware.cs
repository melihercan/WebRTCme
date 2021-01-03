using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Middleware.Blazor
{
    public static class WebRtcMiddleware
    {
        internal static IWebRtc WebRtc { get; private set; }

        public static void Initialize()
        {
            WebRtc = CrossWebRtc.Current;
        }

        public static void Cleanup()
        {
            WebRtc.Cleanup();
        }

    }
}
