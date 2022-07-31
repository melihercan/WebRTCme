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
    internal class Navigator : NativeBase, INavigator
    {
        public Navigator(IJSRuntime jsRuntime) : this(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", "navigator"))
        {
        }

        public Navigator(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
        }

        public IMediaDevices MediaDevices => new MediaDevices(JsRuntime);
    }
}
