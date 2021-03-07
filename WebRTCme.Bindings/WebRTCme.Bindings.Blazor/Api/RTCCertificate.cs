using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class RTCCertificate : ApiBase, IRTCCertificate
    {
        public static IRTCCertificate Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefCertificate) => 
            new RTCCertificate(jsRuntime, jsObjectRefCertificate);

        private RTCCertificate(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public ulong Expires => GetNativeProperty<ulong>("expires");

    }
}
