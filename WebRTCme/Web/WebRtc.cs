using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using WebRtcJsInterop;
using Microsoft.JSInterop;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public Task<IWindow> Window()
        {
            throw new NotImplementedException();
        }

        public async Task<IWindow> Window(IJSRuntime jsRuntime)
        {
            return await WebRtcJsInterop.Window.New(jsRuntime);
        }
    }
}
