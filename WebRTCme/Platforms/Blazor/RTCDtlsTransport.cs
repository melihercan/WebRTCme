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
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCDtlsTransport : NativeBase, IRTCDtlsTransport
    {
        public RTCDtlsTransport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCIceTransport IceTransport => 
            new RTCIceTransport(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "iceTransport"));

        public RTCDtlsTransportState State => GetNativeProperty<RTCDtlsTransportState>("state");
    }
}
