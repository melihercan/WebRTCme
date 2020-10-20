using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrackAsync
    {
        private MediaStreamTrack(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        internal static IMediaStreamTrackAsync NewAsync(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStreamTrack)
        {
            var mediaStreamTrack = new MediaStreamTrack(jsRuntime, jsObjectRefMediaStreamTrack);
            return mediaStreamTrack;
        }
    }
}
