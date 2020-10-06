using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWebRtc
    {
        //void Initialize(IServiceProvider serviceProvider = null);

        INavigator Navigator { get; }
        
        IRTCPeerConnection CreateRTCPeerConnection();



        //Task<object> GetUserMedia(IJSRuntime jsRuntime);


    }
}
