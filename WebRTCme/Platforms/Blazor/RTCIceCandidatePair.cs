using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCIceCandidatePair : NativeBase, IRTCIceCandidatePair
    {
        public RTCIceCandidatePair(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCIceCandidate Local 
        {
            get => new RTCIceCandidate(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "local"));
            set => SetNativeProperty("local", value);
        }
        
        public IRTCIceCandidate Remote 
        {
            get => new RTCIceCandidate(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "remote"));
            set => SetNativeProperty("remote", value);
        }
    }
}
