﻿using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRtcBindingsWeb.Extensions;
using WebRTCme;
using System.Linq;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCIceTransport : ApiBase, IRTCIceTransport
    {
        public static IRTCIceTransport Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) =>
            new RTCIceTransport(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCIceTransport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListener("ongatheringstatechange", OnGatheringStateChange);
            AddNativeEventListener("onselectedcandidatepairchange", OnSelectedCandidatePairChange);
            AddNativeEventListener("onstatechange", OnStateChange);
        }

        public RTCIceComponent Component => GetNativeProperty<RTCIceComponent>("component");

        public RTCIceGatheringState GatheringState => GetNativeProperty<RTCIceGatheringState>("gatheringState");

        public RTCIceRole Role => GetNativeProperty<RTCIceRole>("role");

        public RTCIceTransportState State => GetNativeProperty<RTCIceTransportState>("state");

        public event EventHandler OnGatheringStateChange;
        public event EventHandler OnSelectedCandidatePairChange;
        public event EventHandler OnStateChange;

        public IRTCIceCandidate[] GetLocalCandidates()
        {
            var jsObjectRefGetLocalCandidates = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getLocalCandidates");
            var jsObjectRefIceCandidateArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetLocalCandidates);
            var iceCandidates = jsObjectRefIceCandidateArray
                .Select(jsObjectRef => RTCIceCandidate.Create(JsRuntime, jsObjectRef))
                .ToArray();
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetLocalCandidates.JsObjectRefId);
            return iceCandidates;
        }

        public RTCIceParameters GetLocalParameters() =>
            JsRuntime.CallJsMethod<RTCIceParameters>(NativeObject, "getLocalParameters");

        public IRTCIceCandidate[] GetRemoteCandidates()
        {
            var jsObjectRefGetRemoteCandidates = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getRemoteCandidates");
            var jsObjectRefIceCandidateArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetRemoteCandidates);
            var iceCandidates = jsObjectRefIceCandidateArray
                .Select(jsObjectRef => RTCIceCandidate.Create(JsRuntime, jsObjectRef))
                .ToArray();
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetRemoteCandidates.JsObjectRefId);
            return iceCandidates;
        }

        public RTCIceParameters GetRemoteParameters() =>
            JsRuntime.CallJsMethod<RTCIceParameters>(NativeObject, "getRemoteParameters");

        public IRTCIceCandidatePair GetSelectedCandidatePair() =>
            RTCIceCandidatePair.Create(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, 
                "getSelectedCandidatePair"));
    }
}
