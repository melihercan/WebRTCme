using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCPeerConnectionIceEvent : NativeBase, IRTCPeerConnectionIceEvent
    {
        public static IRTCPeerConnectionIceEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) =>
            new RTCPeerConnectionIceEvent(jsRuntime, jsObjectRef);

        public RTCPeerConnectionIceEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        { }

        public IRTCIceCandidate Candidate
        {
            get
            {
                // 'null' is valid and indicates end of ICE gathering process.
                var jsPropertyObjectRef = JsRuntime.GetJsPropertyObjectRef(NativeObject, "candidate");
                return jsPropertyObjectRef == null ? null : new RTCIceCandidate(JsRuntime, jsPropertyObjectRef);
            }
        }
    }
}
