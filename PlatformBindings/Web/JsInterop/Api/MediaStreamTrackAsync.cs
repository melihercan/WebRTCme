using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaStreamTrackAsync : BaseApi, IMediaStreamTrackAsync
    {
        private MediaStreamTrackAsync(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }





        internal static IMediaStreamTrackAsync NewAsync(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStreamTrack)
        {
            var mediaStreamTrack = new MediaStreamTrackAsync(jsRuntime, jsObjectRefMediaStreamTrack);
            return mediaStreamTrack;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }
    }
}
