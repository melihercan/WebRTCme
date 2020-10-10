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
            var jsObjectRefStream = await JsRuntime.CallJsMethodAsync(JsObjectRef, "getUserMedia", constraints);
            return MediaStream.New(JsRuntime, jsObjectRefStream);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }

        internal static async Task<IMediaDevices> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsProperty(null, "navigator.mediaDevices");
            var mediaDevices = new MediaDevices(jsRuntime, jsObjectRef);
            return mediaDevices;
        }

    }
}
