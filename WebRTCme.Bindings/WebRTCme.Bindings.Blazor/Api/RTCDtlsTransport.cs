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
    internal class RTCDtlsTransport : ApiBase, IRTCDtlsTransport
    {
        public static IRTCDtlsTransport Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefDtlsTransport) => 
            new RTCDtlsTransport(jsRuntime, jsObjectRefDtlsTransport);

        private RTCDtlsTransport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCIceTransport IceTransport => 
            RTCIceTransport.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "iceTransport"));

        public RTCDtlsTransportState State => GetNativeProperty<RTCDtlsTransportState>("state");
    }
}
