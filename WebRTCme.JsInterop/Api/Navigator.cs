using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop
{
    internal class Navigator : INavigator
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigatorInterop _navigatorInterop;

        internal Navigator(IJSRuntime jsRuntime, NavigatorInterop navigatorInterop)
        {
            _jsRuntime = jsRuntime;
            _navigatorInterop = navigatorInterop;
        }


        public IMediaDevices MediaDevices => new MediaDevices();


    }
}
