using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using System.Linq;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCIceTransport : NativeBase, IRTCIceTransport
    {
        public RTCIceTransport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListener("gatheringstatechange", (s, e) => OnGatheringStateChange?.Invoke(s, e));
            AddNativeEventListener("selectedcandidatepairchange", (s, e) => 
                OnSelectedCandidatePairChange?.Invoke(s, e));
            AddNativeEventListener("statechange", (s, e) => OnStateChange?.Invoke(s, e));
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
                .Select(jsObjectRef => new RTCIceCandidate(JsRuntime, jsObjectRef))
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
                .Select(jsObjectRef => new RTCIceCandidate(JsRuntime, jsObjectRef))
                .ToArray();
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetRemoteCandidates.JsObjectRefId);
            return iceCandidates;
        }

        public RTCIceParameters GetRemoteParameters() =>
            JsRuntime.CallJsMethod<RTCIceParameters>(NativeObject, "getRemoteParameters");

        public IRTCIceCandidatePair GetSelectedCandidatePair() =>
            new RTCIceCandidatePair(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, 
                "getSelectedCandidatePair"));
    }
}
