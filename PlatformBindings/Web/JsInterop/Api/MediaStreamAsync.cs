using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaStreamAsync : BaseApi, IMediaStreamAsync
    {
        private MediaStreamAsync(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }

        public async Task<List<IMediaStreamTrackAsync>> GetTracksAsync()
        {
            var jsObjectRef = await JsRuntime.CallJsMethod<JsObjectRef>(JsObjectRef, "getTracks", new object[] { });
            var jsObjectRefMediaStreamTrackArray = await JsRuntime.GetJsPropertyArray(jsObjectRef, null);
            var mediaStreamTracks = new List<IMediaStreamTrackAsync>();
            foreach(var jsObjectRefMediaStreamTrack in jsObjectRefMediaStreamTrackArray)
            {
                mediaStreamTracks.Add(MediaStreamTrackAsync.NewAsync(JsRuntime, jsObjectRefMediaStreamTrack));
            }
            return mediaStreamTracks;
        }

        public async Task SetElementReferenceSrcObjectAsync(object/*ElementReference*/ elementReference)
        {
            await JsRuntime.SetJsProperty(elementReference, "srcObject", JsObjectRef);

            //await JsRuntime.InvokeVoidAsync(
            //    "objectRef.set",
            //    new object[]
            //    {
            //        elementReference,
            //        "srcObject",
            //        JsObjectRef
            //    });

        }

        internal static async Task<IMediaStreamAsync> NewAsync(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.CreateJsObject("window", "MediaStream");
            var mediaStream = new MediaStreamAsync(jsRuntime, jsObjectRef);
            return mediaStream;
        }
        internal static IMediaStreamAsync New(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStream)
        {
            var mediaStream = new MediaStreamAsync(jsRuntime, jsObjectRefMediaStream);
            return mediaStream;
        }

    }
}
