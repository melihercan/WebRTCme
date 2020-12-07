using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRtcBindingsWeb.Extensions;
using WebRTCme;
using System.Linq;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCTrackEvent : ApiBase, IRTCTrackEvent
    {

        public static IRTCTrackEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCTrackEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCTrackEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCRtpReceiver Receiver 
        {
            get => RTCRtpReceiver.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "receiver"));
            set => SetNativeProperty("receiver", value.NativeObject);
        }
        
        public IMediaStream[] Streams 
        {
            get
            {
                var jsObjectRefStreams = JsRuntime.GetJsPropertyObjectRef(NativeObject, "streams");
                var jsObjectRefStreamsArray = JsRuntime.GetJsPropertyArray(jsObjectRefStreams);
                var streams = jsObjectRefStreamsArray
                    .Select(jsObjectRef => MediaStream.Create(JsRuntime, jsObjectRef))
                    .ToArray();
                JsRuntime.DeleteJsObjectRef(jsObjectRefStreams.JsObjectRefId);
                return streams;
            }
            set => SetNativeProperty("streams", value.Select(stream => stream.NativeObject).ToArray());
        }

        public IMediaStreamTrack Track 
        {
            get => MediaStreamTrack.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "track"));
            set => SetNativeProperty("track", value.NativeObject);
        }
        
        public IRTCRtpTransceiver Transceiver 
        {
            get => RTCRtpTransceiver.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "transceiver"));
            set => SetNativeProperty("transceiver", value.NativeObject);
        }
    }
}
