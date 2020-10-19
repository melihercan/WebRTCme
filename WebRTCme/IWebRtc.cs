using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWebRtc
    {
        IWindow Window { get; }

        Task<IWindowAsync> WindowAsync(IJSRuntime jsRuntime);
    }
}
