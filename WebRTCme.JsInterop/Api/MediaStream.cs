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

        internal static async Task<IMediaStream> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.CreateJsObject(null, "MediaStream");
            var mediaStream = new MediaStream(jsRuntime, jsObjectRef);
            return mediaStream;
        }
        internal static IMediaStream New(IJSRuntime jsRuntime, JsObjectRef jsObjectRefStream)
        {
            var mediaStream = new MediaStream(jsRuntime, jsObjectRefStream);
            return mediaStream;
        }

        public async Task SetMediaSourceAsync(object/*ElementReference*/ elementReference)
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
    }
}
