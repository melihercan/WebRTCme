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

        public async Task<IWindow> Window(IJSRuntime jsRuntime)
        {
            return await WebRtcJsInterop.Api.Window.CreateAsync(jsRuntime);
        }
    }
}
