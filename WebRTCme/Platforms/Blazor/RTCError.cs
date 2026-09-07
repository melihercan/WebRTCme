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
    internal class RTCError : NativeBase, IRTCError
    {
        public static IRTCError Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) =>
            new RTCError(jsRuntime, jsObjectRef);

        public RTCError(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public RTCErrorDetailType ErrorDetail => GetNativeProperty<RTCErrorDetailType>("errorDetail");

        public string Message => GetNativeProperty<string>("message");

        public string Name => GetNativeProperty<string>("name");

        public int? SdpLineNumber => GetNativeProperty<int?>("sdpLineNumber");

        public int? SctpCauseCode => GetNativeProperty<int?>("sctpCauseCode");

        public uint? ReceivedAlert => GetNativeProperty<uint?>("receivedAlert");

        public uint? SentAlert => GetNativeProperty<uint?>("sentAlert");
    }
}
