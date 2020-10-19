using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtc.iOS;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public IWindow Window => new Window();

        public Task<IWindowAsync> WindowAsync(IJSRuntime jsRuntime) => throw new NotImplementedException();
    }
}
