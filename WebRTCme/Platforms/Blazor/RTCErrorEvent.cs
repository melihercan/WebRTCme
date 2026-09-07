using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCErrorEvent : NativeBase, IRTCErrorEvent
    {
        public static IRTCErrorEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) =>
            new RTCErrorEvent(jsRuntime, jsObjectRef);

        public RTCErrorEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCError Error =>
            new RTCError(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "error"));
    }
}
