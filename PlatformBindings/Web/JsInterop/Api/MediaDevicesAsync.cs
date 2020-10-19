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
    internal class MediaDevicesAsync : BaseApi, IMediaDevicesAsync
    {
        private MediaDevicesAsync(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        async Task<IMediaStreamAsync> IMediaDevicesAsync.GetUserMediaAsync(MediaStreamConstraints constraints)
        {
            var jsObjectRefMediaStream = await JsRuntime.CallJsMethodAsync<JsObjectRef>(JsObjectRef, 
                "getUserMedia", constraints);
            return MediaStreamAsync.New(JsRuntime, jsObjectRefMediaStream);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }

        internal static async Task<IMediaDevicesAsync> NewAsync(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsPropertyObjectRef("window", "navigator.mediaDevices");
            var mediaDevices = new MediaDevicesAsync(jsRuntime, jsObjectRef);
            return mediaDevices;
        }

    }
}
