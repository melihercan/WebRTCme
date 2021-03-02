using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRtcBindingsBlazor.Interops;
using WebRtcBindingsBlazor.Extensions;
using WebRTCme;

namespace WebRtcBindingsBlazor.Api
{
    internal class RTCIceCandidatePair : ApiBase, IRTCIceCandidatePair
    {
        public static IRTCIceCandidatePair Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) =>
            new RTCIceCandidatePair(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCIceCandidatePair(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCIceCandidate Local 
        {
            get => RTCIceCandidate.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "local"));
            set => SetNativeProperty("local", value);
        }
        
        public IRTCIceCandidate Remote 
        {
            get => RTCIceCandidate.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "remote"));
            set => SetNativeProperty("remote", value);
        }
    }
}
