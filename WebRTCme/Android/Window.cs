using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.Android
{
    internal class Window : ApiBase, IWindow
    {
        public Task<INavigator> Navigator() => Task.FromResult(new Navigator() as INavigator);

        public Task<IRTCPeerConnection> RTCPeerConnection(RTCConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
