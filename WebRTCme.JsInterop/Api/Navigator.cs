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
    internal class Navigator : BaseApi, INavigator
    {
        private Navigator(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        async Task<IMediaDevices> INavigator.MediaDevices() => await Api.MediaDevices.New(JsRuntime);

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }

        internal static async Task<INavigator> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsProperty(null, "navigator");
            var navigator = new Navigator(jsRuntime, jsObjectRef);
            return navigator;
        }

    }
}
