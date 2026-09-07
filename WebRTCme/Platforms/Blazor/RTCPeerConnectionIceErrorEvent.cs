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
    internal class RTCPeerConnectionIceErrorEvent : NativeBase, IRTCPeerConnectionIceErrorEvent
    {
        public static IRTCPeerConnectionIceErrorEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) =>
            new RTCPeerConnectionIceErrorEvent(jsRuntime, jsObjectRef);

        public RTCPeerConnectionIceErrorEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef)
            : base(jsRuntime, jsObjectRef) { }

        public string Address => GetNativeProperty<string>("address");

        public ushort? Port => GetNativeProperty<ushort?>("port");

        public string Url => GetNativeProperty<string>("url");

        public ushort ErrorCode => GetNativeProperty<ushort>("errorCode");

        public string ErrorText => GetNativeProperty<string>("errorText");
    }
}
