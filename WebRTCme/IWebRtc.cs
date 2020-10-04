using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IWebRtc
    {
        //void Initialize(IServiceProvider serviceProvider = null);
        
        IRTCPeerConnection CreateRTCPeerConnection(IJSRuntime jsRuntime = null);


    }
}
