using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class RTCDataChannelEvent : ApiBase, IRTCDataChannelEvent
    {
        public static IRTCDataChannelEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCDataChannelEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCDataChannelEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCDataChannel Channel => 
            RTCDataChannel.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "channel"));
    }
}
