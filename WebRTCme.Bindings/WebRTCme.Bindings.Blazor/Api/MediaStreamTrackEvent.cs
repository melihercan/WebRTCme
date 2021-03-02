using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcMeBindingsBlazor.Interops;
using WebRtcMeBindingsBlazor.Extensions;
using WebRTCme;

namespace WebRtcMeBindingsBlazor.Api
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
