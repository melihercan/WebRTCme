using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWebRtc
    {
        Task<IWindow> CreateWindow(IJSRuntime jsRuntime = null);

        Task CleanupAsync();
    }
}
