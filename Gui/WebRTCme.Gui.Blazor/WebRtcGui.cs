using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

//// TODO: REFACTOR TO WebRTCme.Blazor
namespace WebRtcGuiBlazor
{
    //// TODO: RENAME TO WebRtcBlazor
    public static class WebRtcGui
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
