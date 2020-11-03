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
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public IWindow Window => new Window();

        public Task<IWindowAsync> WindowAsync(IJSRuntime jsRuntime) => throw new NotImplementedException();
    }
}
