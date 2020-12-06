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
    internal class RTCSessionDescription : ApiBase, IRTCSessionDescription
    {
        public static IRTCSessionDescription Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefSessionDescription) 
            => new RTCSessionDescription(jsRuntime, jsObjectRefSessionDescription);

        private RTCSessionDescription(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public RTCSdpType Type => GetNativeProperty<RTCSdpType>("type");

        public string Sdp => GetNativeProperty<string>("sdp");

        public string ToJson() => JsonSerializer.Serialize(this);
    }
}
