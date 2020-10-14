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
    internal class MediaDevices : BaseApi, IMediaDevices
    {
        private MediaDevices(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        async Task<IMediaStream> IMediaDevices.GetUserMedia(MediaStreamConstraints constraints)
        {
            var jsObjectRefMediaStream = await JsRuntime.CallJsMethodAsync<JsObjectRef>(JsObjectRef, 
                "getUserMedia", constraints);
            return MediaStream.New(JsRuntime, jsObjectRefMediaStream);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }

        internal static async Task<IMediaDevices> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsPropertyObjectRef("window", "navigator.mediaDevices");
            var mediaDevices = new MediaDevices(jsRuntime, jsObjectRef);
            return mediaDevices;
        }

    }
}
