using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.MediaSoupClient
{
    public static class Registry
    {
        public static IWebRtc WebRtc { get; set; }
        public static IJSRuntime JsRuntime { get; set; }
    }
}
