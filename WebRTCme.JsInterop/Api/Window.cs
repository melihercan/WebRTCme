using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop
{
    public class Window : IWindow
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly WindowInterop _windowInterop;

        private Window(IJSRuntime jsRuntime, WindowInterop windowInterop)
        {
            _jsRuntime = jsRuntime;
            _windowInterop = windowInterop;
        }

        public static async Task<IWindow> New(IJSRuntime jsRuntime)
        {
            var windowInterop = await jsRuntime.Window();
            var window = new Window(jsRuntime, windowInterop);
            return window;
        }


        public INavigator Navigator => new Navigator();

        public IRTCPeerConnection RTCPeerConnection()
        {
            return new RTCPeerConnection();
        }
    }
}
