using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop
{
    internal class MediaDevices : IMediaDevices
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly JsObjectRef _jsObjectRef;
        private JsObjectRef _jsObjectRefStream;

        private MediaDevices(IJSRuntime jsRuntime, JsObjectRef jsObjectRef)
        {
            _jsRuntime = jsRuntime;
            _jsObjectRef = jsObjectRef;
        }

        async Task<IMediaStream> IMediaDevices.GetUserMedia(MediaStreamConstraints constraints)
        {
            _jsObjectRefStream = await _jsRuntime.CallJsMethodAsync(_jsObjectRef, "getUserMedia", constraints);
            return new MediaStream();
        }

        public async ValueTask DisposeAsync() => await _jsRuntime.DeleteJsObject(_jsObjectRef.JsObjectRefId);

        internal static async Task<IMediaDevices> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsProperty(null, "navigator.mediaDevices");
            var mediaDevices = new MediaDevices(jsRuntime, jsObjectRef);
            return mediaDevices;
        }

    }
}
