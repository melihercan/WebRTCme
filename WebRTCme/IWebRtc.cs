using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWebRtc
    {
        //void Initialize(IServiceProvider serviceProvider = null);
        
        IRTCPeerConnection CreateRTCPeerConnection(IJSRuntime jsRuntime = null);

        Task<object> GetUserMedia(IJSRuntime jsRuntime);


    }
}
