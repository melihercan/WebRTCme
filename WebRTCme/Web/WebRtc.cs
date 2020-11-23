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

        public static Task<IWebRtc> CreateAsync()
        {
            var ret = new WebRtc();
            return ret.InitializeAsync();
        }

        private Task<IWebRtc> InitializeAsync()
        {
            return Task.FromResult(this as IWebRtc);
        }

        public Task CleanupAsync()
        {
            return Task.CompletedTask;
        }

        public IWindow Window(IJSRuntime jsRuntime) => WebRtcJsInterop.Api.Window.Create(jsRuntime);
    }
}
