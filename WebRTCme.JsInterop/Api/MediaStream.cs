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
    internal class MediaStream : BaseApi, IMediaStream
    {
        private MediaStream(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }

        public async Task<List<IMediaStreamTrack>> GetTracks()
        {
            var jsObjectRef = await JsRuntime.CallJsMethod<JsObjectRef>(JsObjectRef, "getTracks", new object[] { });
            var jsObjectRefMediaStreamTrackArray = await JsRuntime.GetJsPropertyObjectRefArray(jsObjectRef, null);
            var mediaStreamTracks = new List<IMediaStreamTrack>();
            foreach(var jsObjectRefMediaStreamTrack in jsObjectRefMediaStreamTrackArray)
            {
                mediaStreamTracks.Add(await MediaStreamTrack.New(JsRuntime, jsObjectRefMediaStreamTrack));
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

        internal static async Task<IMediaStream> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.CreateJsObject("window", "MediaStream");
            var mediaStream = new MediaStream(jsRuntime, jsObjectRef);
            return mediaStream;
        }
        internal static IMediaStream New(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStream)
        {
            var mediaStream = new MediaStream(jsRuntime, jsObjectRefMediaStream);
            return mediaStream;
        }

    }
}
