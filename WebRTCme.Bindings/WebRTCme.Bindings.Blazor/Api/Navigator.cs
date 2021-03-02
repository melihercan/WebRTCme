using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcMeBindingsBlazor.Extensions;
using WebRtcMeBindingsBlazor.Interops;
using WebRTCme;

namespace WebRtcMeBindingsBlazor.Api
{
    internal class Navigator : ApiBase, INavigator
    {
        public static INavigator Create(IJSRuntime jsRuntime) =>
            new Navigator(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", "navigator"));
        
        private Navigator(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IMediaDevices MediaDevices => Api.MediaDevices.Create(JsRuntime);
    }
}
