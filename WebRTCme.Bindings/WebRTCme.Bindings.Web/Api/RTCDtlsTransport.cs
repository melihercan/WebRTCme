using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCDtlsTransport : ApiBase, IRTCDtlsTransport
    {

        internal static IRTCDtlsTransport Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefDtlsTransport) => 
            new RTCDtlsTransport(jsRuntime, jsObjectRefDtlsTransport);

        private RTCDtlsTransport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


    }
}
