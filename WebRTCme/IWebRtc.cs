using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWebRtc
    {
        //void Initialize(IServiceProvider serviceProvider = null);

        IWindow Window { get; }
        
        //IRTCPeerConnection CreateRTCPeerConnection();



        //Task<object> GetUserMedia(IJSRuntime jsRuntime);


    }
}
