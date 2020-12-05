using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Extensions;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class Navigator : ApiBase, INavigator
    {
        public static INavigator Create(IJSRuntime jsRuntime)
        {
            var jsObjectRef = jsRuntime.GetJsPropertyObjectRef("window", "navigator");
            var navigator = new Navigator(jsRuntime, jsObjectRef);
            return navigator;
        }
        private Navigator(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IMediaDevices MediaDevices => 
            Api.MediaDevices.Create(JsRuntime);

    }
}
