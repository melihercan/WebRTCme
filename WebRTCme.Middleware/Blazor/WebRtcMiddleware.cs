using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

//// TODO: REFACTOR TO WebRTCme.Blazor
namespace WebRTCme.Middleware.Blazor
{
    //// TODO: RENAME TO WebRtcBlazor
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
