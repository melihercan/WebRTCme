using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class VideoCapturer : ApiBase, IVideoCapturer
    {
        public static IVideoCapturer Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) =>
            new VideoCapturer(jsRuntime, jsObjectRef);

        private VideoCapturer(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }
    }
}
