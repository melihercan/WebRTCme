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
    internal class MediaDevices : ApiBase, IMediaDevicesAsync
    {
        private MediaDevices(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        async Task<IMediaStreamAsync> IMediaDevicesAsync.GetUserMediaAsync(MediaStreamConstraints constraints)
        {
            var jsObjectRefMediaStream = await JsRuntime.CallJsMethodAsync<JsObjectRef>(NativeObject, 
                "getUserMedia", constraints);
            return MediaStream.New(JsRuntime, jsObjectRefMediaStream);
        }

        internal static async Task<IMediaDevicesAsync> NewAsync(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsPropertyObjectRef("window", "navigator.mediaDevices");
            var mediaDevices = new MediaDevices(jsRuntime, jsObjectRef);
            return mediaDevices;
        }

        public Task<IEnumerable<IMediaDeviceInfo>> EnumerateDevices()
        {
            throw new NotImplementedException();
        }
    }
}
