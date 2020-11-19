using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class Window : ApiBase, IWindow
    {
        public Task<INavigator> Navigator() => Task.FromResult(new Navigator() as INavigator);

        public Task<IRTCPeerConnection> RTCPeerConnection(RTCConfiguration configuration) => 
            Task.FromResult(new RTCPeerConnection(configuration) as IRTCPeerConnection);
    }
}
