using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class Window : ApiBase, IWindow
    {
        public INavigator Navigator => new Navigator();

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
