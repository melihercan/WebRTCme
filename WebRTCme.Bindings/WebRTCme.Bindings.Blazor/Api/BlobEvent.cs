using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class BlobEvent : ApiBase, IBlobEvent
    {
        public static IBlobEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefNativeBlobEvent) =>
            new BlobEvent(jsRuntime, jsObjectRefNativeBlobEvent);

        private BlobEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        public IBlob Data => Blob.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "data"));

        public double Timecode => GetNativeProperty<double>("timecode");
    }
}
