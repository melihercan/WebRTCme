using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.JSInterop;


namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        private WebRtc() { }

        public static IWebRtc Create() => new WebRtc();

        public void Cleanup() { }

        public IWindow Window(IJSRuntime jsRuntime) => WebRtcBindingsWeb.Api.Window.Create(jsRuntime);
    }
}
