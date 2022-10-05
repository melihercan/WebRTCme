using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public static IWebRtc Create() => new WebRtc();

        public void Dispose()
        {
        }

#if NETSTANDARD
        public IWindow Window(IJSRuntime jsRuntime) => throw new NotImplementedException();
#else
        public IWindow Window(IJSRuntime jsRuntime) => new WebRTCme.Bindings.Maui.Windows.Api.Window();
#endif

    }
}
