using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class DOMException : NativeBase, IDOMException
    {
        public static IDOMException Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefNativeDomException) =>
            new DOMException(jsRuntime, jsObjectRefNativeDomException);

        public DOMException(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        public string Message => GetNativeProperty<string>("message");

        public string Name => GetNativeProperty<string>("name");
    }
}
