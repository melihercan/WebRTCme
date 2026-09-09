using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup.Proxy.Stub;

namespace WebRTCme.Connection.MediaSoup.Proxy
{
    static class Registry
    {
        public static IWebRtc WebRtc { get; set; }
        public static ILogger<MediaSoupStub> Logger { get; set; }
        public static IJSRuntime JsRuntime { get; set; }
    }
}
