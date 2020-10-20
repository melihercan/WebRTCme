using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Android
{
    internal class Window : ApiBase<object>, IWindow
    {
        public INavigator Navigator => new Navigator();

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration)
        {
            throw new NotImplementedException();
        }

    }
}
