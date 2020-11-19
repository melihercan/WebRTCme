using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtcGuiXamarin
{
    public static class WebRtcGui
    {
        internal static IWebRtc WebRtc { get; private set; }


        public static async Task InitializeAsync()
        {
            WebRtc = await CrossWebRtc.Instance;
        }

        public static async Task CleanupAsync()
        {
            await WebRtc.CleanupAsync();
        }
    }
}
