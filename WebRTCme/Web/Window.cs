using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Web
{
    internal class Window : IWindow
    {
        public Window()
        {
        }

        public INavigator Navigator => new Navigator();

        public IRTCPeerConnection NewRTCPeerConnection()
        {
            return new RTCPeerConnection();
        }
    }
}
