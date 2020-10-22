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
    internal class MediaStream : ApiBase, IMediaStreamAsync
    {
        private MediaStream(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        public async Task<List<IMediaStreamTrackAsync>> GetTracksAsync()
        {
            var jsObjectRef = await JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getTracks");
            var jsObjectRefMediaStreamTrackArray = await JsRuntime.GetJsPropertyArray(jsObjectRef);
            var mediaStreamTracks = new List<IMediaStreamTrackAsync>();
            foreach(var jsObjectRefMediaStreamTrack in jsObjectRefMediaStreamTrackArray)
            {
                mediaStreamTracks.Add(MediaStreamTrack.NewAsync(JsRuntime, jsObjectRefMediaStreamTrack));
            }
            return mediaStreamTracks;
        }

        public async Task SetElementReferenceSrcObjectAsync(object/*ElementReference*/ elementReference)
        {
            await JsRuntime.SetJsProperty(elementReference, "srcObject", NativeObject);

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
            var mediaStream = new MediaStream(jsRuntime, jsObjectRef);
            return mediaStream;
        }
        internal static IMediaStreamAsync New(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStream)
        {
            var mediaStream = new MediaStream(jsRuntime, jsObjectRefMediaStream);
            return mediaStream;
        }

    }
}
