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
    internal class RTCRtpTransceiver : NativeBase, IRTCRtpTransceiver
    {
        public RTCRtpTransceiver(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public RTCRtpTransceiverDirection CurrentDirection =>
            GetNativeProperty<RTCRtpTransceiverDirection>("currentDirection");

        public RTCRtpTransceiverDirection Direction 
        { 
            get => GetNativeProperty<RTCRtpTransceiverDirection>("direction");
            set => SetNativeProperty("direction", value);
        }

        public string Mid => GetNativeProperty<string>("mid");

        public IRTCRtpReceiver Receiver => 
            new RTCRtpReceiver(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "receiver"));

        public IRTCRtpSender Sender =>
            new RTCRtpSender(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "sender"));

        public bool Stopped => GetNativeProperty<bool>("stopped");


        public void SetCodecPreferences(RTCRtpCodecCapability[] codecs) =>
            JsRuntime.CallJsMethodVoid(NativeObject, "setCodecPreferences", codecs);

        public void Stop() => JsRuntime.CallJsMethodVoid(NativeObject, "stop");
    }
}
