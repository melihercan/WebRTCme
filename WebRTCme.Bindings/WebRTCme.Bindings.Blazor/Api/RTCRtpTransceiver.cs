using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsBlazor.Interops;
using WebRtcBindingsBlazor.Extensions;
using WebRTCme;

namespace WebRtcBindingsBlazor.Api
{
    internal class RTCRtpTransceiver : ApiBase, IRTCRtpTransceiver
    {
        public static IRTCRtpTransceiver Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtpTransceiver) =>
            new RTCRtpTransceiver(jsRuntime, jsObjectRefRtpTransceiver);

        private RTCRtpTransceiver(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public RTCRtpTransceiverDirection CurrentDirection =>
            GetNativeProperty<RTCRtpTransceiverDirection>("currentDirection");

        public RTCRtpTransceiverDirection Direction 
        { 
            get => GetNativeProperty<RTCRtpTransceiverDirection>("direction");
            set => SetNativeProperty("direction", value);
        }

        public string Mid => GetNativeProperty<string>("mid");

        public IRTCRtpReceiver Receiver => 
            RTCRtpReceiver.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "receiver"));

        public IRTCRtpSender Sender =>
            RTCRtpSender.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "sender"));

        public bool Stopped => GetNativeProperty<bool>("stopped");


        public void SetCodecPreferences(RTCRtpCodecCapability[] codecs) =>
            JsRuntime.CallJsMethodVoid(NativeObject, "setCodecPreferences", codecs);

        public void Stop() => JsRuntime.CallJsMethodVoid(NativeObject, "stop");
    }
}
