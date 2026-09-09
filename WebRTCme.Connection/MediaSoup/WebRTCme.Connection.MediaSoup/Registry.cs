using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup;

namespace WebRTCme.Connection.MediaSoup
{
    static class Registry
    {
        public static IWebRtc WebRtc { get; set; }
        public static ILogger<MediaSoupStub> Logger { get; set; }
        public static IJSRuntime JsRuntime { get; set; }
    }
}
