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
        internal static IRTCSessionDescription Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefSessionDescription)
        {
            return new RTCSessionDescription(jsRuntime, jsObjectRefSessionDescription);
        }

        private RTCSessionDescription(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public RTCSdpType Type { get; }

        public string Sdp { get; }

        public string ToJson() => JsonSerializer.Serialize(this);
    }
}
