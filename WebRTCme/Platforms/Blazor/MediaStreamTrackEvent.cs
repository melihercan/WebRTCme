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
    internal class MediaStreamTrackEvent : NativeBase, IMediaStreamTrackEvent
    {

        public static IMediaStreamTrackEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new MediaStreamTrackEvent(jsRuntime, jsObjectRefRtcStatsReport);

        public MediaStreamTrackEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IMediaStreamTrack Track => 
            new MediaStreamTrack(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "track"));
    }
}
