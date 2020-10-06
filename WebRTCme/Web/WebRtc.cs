using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using WebRtcJsInterop;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public async Task<IWindow> NewWindow()
        {
            return await Window.New();
        }
    }
}
