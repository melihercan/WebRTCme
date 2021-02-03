                                                                                                                      using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class VideoView : ApiBase, IVideoView
    {
        public static IVideoView Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) =>
            new VideoView(jsRuntime, jsObjectRef);

        private VideoView(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }
    }
}
