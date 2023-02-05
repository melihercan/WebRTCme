using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Blazor;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;


namespace WebRTCme
{
    public static class BlazorSupport
    {
        public static void SetVideoSource(IJSRuntime jsRuntime, ElementReference videoElementReference,
            IMediaStream mediaStream, bool muted = false)
        {
            jsRuntime.SetJsProperty(videoElementReference, "srcObject", ((MediaStream)mediaStream).NativeObject);

            //jsRuntime.SetJsProperty(videoElementReference, "autoplay", true);
            //jsRuntime.SetJsProperty(videoElementReference, "playsInline", true);
            jsRuntime.SetJsProperty(videoElementReference, "muted", muted);

        }
    }
}
