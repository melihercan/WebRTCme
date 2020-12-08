using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class Window : ApiBase, IWindow
    {
        public static IWindow Create() => new Window();

        public INavigator Navigator() => iOS.Navigator.Create();

        public IRTCConfiguration RTCConfiguration() => iOS.RTCConfiguration.Create();

        public IRTCPeerConnection RTCPeerConnection(IRTCConfiguration configuration) => 
            iOS.RTCPeerConnection.Create(configuration);
    }
}
