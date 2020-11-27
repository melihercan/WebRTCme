using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWindow : INativeObject
    {
        INavigator Navigator { get; }

        IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration = null);
    }

}
