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
        public Task<INavigator> CreateNavigator() => Task.FromResult(new Navigator() as INavigator);

        public Task<IRTCPeerConnection> CreateRTCPeerConnection(RTCConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
