using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
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
