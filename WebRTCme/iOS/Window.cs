using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class Window : ApiBase, IWindow
    {
        public static Task<IWindow> CreateAsync()
        {
            var ret = new Window();
            return ret.InitializeAsync();
        }

        private Window() { }

        private Task<IWindow> InitializeAsync() => Task.FromResult(this as IWindow);

        public Task<INavigator> CreateNavigator() => Task.FromResult(new Navigator() as INavigator);

        public Task<IRTCPeerConnection> CreateRTCPeerConnection(RTCConfiguration configuration) => 
            iOS.RTCPeerConnection.CreateAsync(configuration);
    }
}
