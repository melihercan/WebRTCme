using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    public class Window : NativeBase, IWindow
    {
        public static IWindow Create(IJSRuntime jsRuntime) =>
            new Window(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", null));

        public Window(IJSRuntime jsRuntime) : this(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", null)) { }

        public Window(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public INavigator Navigator() => new Navigator(JsRuntime);

        public IMediaStream MediaStream() =>   new MediaStream(JsRuntime);

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration) =>
            new RTCPeerConnection(JsRuntime, configuration);


        //// TODO: REFACTOR WHOLE BLAZOR API BY USING System.Private.Runtime.InteropServices.JavaScript and use HostObject.
        ////public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null)
        ////{
        ////return new MediaRecorder(stream, options);
        ////}
        ///

        public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null) =>
            new MediaRecorder(JsRuntime, stream, options);

    }
}
