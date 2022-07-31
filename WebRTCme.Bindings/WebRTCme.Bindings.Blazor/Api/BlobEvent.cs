using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class BlobEvent : NativeBase, IBlobEvent
    {
        public static IBlobEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefNativeBlobEvent) =>
            new BlobEvent(jsRuntime, jsObjectRefNativeBlobEvent);

        public BlobEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        public IBlob Data => new Blob(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "data"));

        public double Timecode => GetNativeProperty<double>("timecode");
    }
}
