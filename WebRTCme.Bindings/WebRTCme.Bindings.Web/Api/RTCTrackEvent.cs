using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCTrackEvent : ApiBase, IRTCTrackEvent
    {

        public static IRTCTrackEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCTrackEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCTrackEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCRtpReceiver Receiver { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IMediaStream[] Streams { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IMediaStreamTrack Track { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IRTCRtpTransceiver Transceiver { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


    }
}
