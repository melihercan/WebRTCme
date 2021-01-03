using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcBindingsWeb.Extensions;
using WebRtcBindingsWeb.Interops;


namespace WebRTCme
{
    public static class PlatformSupport
    {
        public static void SetVideoSource(IJSRuntime jsRuntime, ElementReference videoElementReference, 
            IMediaStream mediaStream)
        {
            jsRuntime.SetJsProperty(videoElementReference, "srcObject", mediaStream.NativeObject);
        }
    }
}
