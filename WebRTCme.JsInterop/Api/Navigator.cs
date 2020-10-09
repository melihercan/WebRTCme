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
    internal class Navigator : INavigator
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly JsObjectRef _jsObjectRef;

        private Navigator(IJSRuntime jsRuntime, JsObjectRef jsObjectRef)
        {
            _jsRuntime = jsRuntime;
            _jsObjectRef = jsObjectRef;
        }

        public async Task<IMediaDevices> MediaDevices() => await WebRtcJsInterop.MediaDevices.New(_jsRuntime);

        public async ValueTask DisposeAsync() => await _jsRuntime.DeleteJsObject(_jsObjectRef.JsObjectRefId);

        internal static async Task<INavigator> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsProperty(null, "navigator");
            var navigator = new Navigator(jsRuntime, jsObjectRef);
            return navigator;
        }
    }
}
