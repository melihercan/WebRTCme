using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtcJsInterop
{
    public class Window : IWindow
    {
        private readonly IJSRuntime _jsRuntime;

        private Window(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public static async Task<IWindow> New(IJSRuntime jsRuntime)
        {
            IWindow window = new Window(jsRuntime);


            return window;
        }


        public INavigator Navigator => new Navigator();

        public IRTCPeerConnection RTCPeerConnection()
        {
            return new RTCPeerConnection();
        }
    }
}
