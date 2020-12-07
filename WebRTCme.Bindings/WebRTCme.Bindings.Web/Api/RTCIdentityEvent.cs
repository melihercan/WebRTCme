using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCIdentityEvent : ApiBase, IRTCIdentityEvent
    {
        public static IRTCIdentityEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCIdentityEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCIdentityEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string Assertion => GetNativeProperty<string>("assertion");
    }
}
