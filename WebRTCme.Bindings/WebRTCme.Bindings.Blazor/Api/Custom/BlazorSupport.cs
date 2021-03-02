using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcBindingsBlazor.Extensions;
using WebRtcBindingsBlazor.Interops;


namespace WebRTCme
{
    public static class BlazorSupport
    {
        public static void SetVideoSource(IJSRuntime jsRuntime, ElementReference videoElementReference, 
            IMediaStream mediaStream)
        {
            jsRuntime.SetJsProperty(videoElementReference, "srcObject", mediaStream.NativeObject);

            //jsRuntime.SetJsProperty(videoElementReference, "autoplay", true);
            //jsRuntime.SetJsProperty(videoElementReference, "playsInline", true);
            //jsRuntime.SetJsProperty(videoElementReference, "muted", true);

        }
    }
}
