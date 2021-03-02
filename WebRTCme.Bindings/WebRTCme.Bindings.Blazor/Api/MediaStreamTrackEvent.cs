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
    internal class MediaStreamTrackEvent : ApiBase, IMediaStreamTrackEvent
    {

        public static IMediaStreamTrackEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new MediaStreamTrackEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private MediaStreamTrackEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IMediaStreamTrack Track => 
            MediaStreamTrack.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "track"));
    }
}
