using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRtcBindingsWeb.Extensions;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCPeerConnectionIceEvent : ApiBase, IRTCPeerConnectionIceEvent
    {
        public static IRTCPeerConnectionIceEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCPeerConnectionIceEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCPeerConnectionIceEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCIceCandidate Candidate => 
            RTCIceCandidate.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "candidate"));


    }
}
