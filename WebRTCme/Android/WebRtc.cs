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

        Task<IWindow> IWebRtc.Window(IJSRuntime jsRuntime)
        {
            return Task.FromResult(new Window() as IWindow);
        }
    }
}
