using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtc.Android;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public static IWebRtc Create() => new WebRtc();

        public void Cleanup() { }

        public IWindow Window(IJSRuntime jsRuntime) => global::WebRtc.Android.Window.Create();
    }
}
