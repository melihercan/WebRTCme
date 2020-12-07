using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWindow : INativeObject
    {
        INavigator Navigator();

        IRTCConfiguration RTCConfiguration();

        IRTCPeerConnection RTCPeerConnection(IRTCConfiguration configuration = null);


    }

}
