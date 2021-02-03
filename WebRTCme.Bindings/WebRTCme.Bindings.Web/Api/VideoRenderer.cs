using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class VideoRenderer : ApiBase, IVideoRenderer
    {
        public static IVideoRenderer Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) =>
            new VideoRenderer(jsRuntime, jsObjectRef);

        private VideoRenderer(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }
    }
}
