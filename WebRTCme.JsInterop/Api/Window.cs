using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtcJsInterop
{
    public class Window : IWindow
    {

        public static async Task<IWindow> New()
        {
            IWindow window = new Window();


            return window;
        }


        public INavigator Navigator => new Navigator();

        public IRTCPeerConnection NewRTCPeerConnection()
        {
            return new RTCPeerConnection();
        }
    }
}
