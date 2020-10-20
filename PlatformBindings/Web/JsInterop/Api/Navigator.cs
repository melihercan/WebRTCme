using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class Navigator : ApiBase, INavigatorAsync
    {
        private Navigator(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        async Task<IMediaDevicesAsync> INavigatorAsync.MediaDevicesAsync() => 
            await Api.MediaDevices.NewAsync(JsRuntime);

        internal static async Task<INavigatorAsync> NewAsync(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsPropertyObjectRef("window", "navigator");
            var navigator = new Navigator(jsRuntime, jsObjectRef);
            return navigator;
        }

    }
}
