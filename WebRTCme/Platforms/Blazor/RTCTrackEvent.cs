using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using System.Linq;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCTrackEvent : NativeBase, IRTCTrackEvent
    {

        public static IRTCTrackEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefNativeTrackEvent) => 
            new RTCTrackEvent(jsRuntime, jsObjectRefNativeTrackEvent);

        public RTCTrackEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCRtpReceiver Receiver 
        {
            get => new RTCRtpReceiver(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "receiver"));
            set => SetNativeProperty("receiver", ((RTCRtpReceiver)value).NativeObject);
        }
        
        public IMediaStream[] Streams 
        {
            get
            {
                var jsObjectRefStreams = JsRuntime.GetJsPropertyObjectRef(NativeObject, "streams");
                var jsObjectRefStreamsArray = JsRuntime.GetJsPropertyArray(jsObjectRefStreams);
                var streams = jsObjectRefStreamsArray
                    .Select(jsObjectRef => new MediaStream(JsRuntime, jsObjectRef))
                    .ToArray();
                JsRuntime.DeleteJsObjectRef(jsObjectRefStreams.JsObjectRefId);
                return streams;
            }
            set => SetNativeProperty("streams", value.Select(stream => ((MediaStream)stream).NativeObject).ToArray());
        }

        public IMediaStreamTrack Track 
        {
            get => new MediaStreamTrack(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "track"));
            set => SetNativeProperty("track", ((MediaStreamTrack)value).NativeObject);
        }
        
        public IRTCRtpTransceiver Transceiver 
        {
            get => new RTCRtpTransceiver(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "transceiver"));
            set => SetNativeProperty("transceiver", ((RTCRtpReceiver)value).NativeObject);
        }
    }
}
