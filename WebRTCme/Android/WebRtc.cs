using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public IWindow Window => throw new NotImplementedException();

        public Task<IWindowAsync> WindowAsync(IJSRuntime jsRuntime)
        {
            throw new NotImplementedException();
        }
    }
}
