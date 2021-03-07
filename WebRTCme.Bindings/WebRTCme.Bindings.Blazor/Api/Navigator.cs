using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class Navigator : ApiBase, INavigator
    {
        public static INavigator Create(IJSRuntime jsRuntime) =>
            new Navigator(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", "navigator"));
        
        private Navigator(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IMediaDevices MediaDevices => Api.MediaDevices.Create(JsRuntime);
    }
}
