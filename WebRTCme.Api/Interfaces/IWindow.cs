using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWindow : INativeObjects
    {
        Task<INavigator> Navigator();

        Task<IRTCPeerConnection> RTCPeerConnection(RTCConfiguration configuration = null);
    }

}
