using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWebRtc
    {
        Task<IWindow> Window(IJSRuntime jsRuntime = null);

        Task CleanupAsync();
    }
}
