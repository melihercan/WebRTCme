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
    internal class Navigator : ApiBase, INavigator
    {
        private Navigator(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public Task<IMediaDevices> MediaDevices => 
            Api.MediaDevices.CreateAsync(JsRuntime);

        public static async Task<INavigator> CreateAsync(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsPropertyObjectRef("window", "navigator");
            var navigator = new Navigator(jsRuntime, jsObjectRef);
            return navigator;
        }
    }
}
