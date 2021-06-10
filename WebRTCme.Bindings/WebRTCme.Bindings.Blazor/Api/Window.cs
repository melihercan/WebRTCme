using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
{
    public class Window : ApiBase, IWindow
    {
        public static IWindow Create(IJSRuntime jsRuntime) =>
            new Window(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", null));

        private Window(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public INavigator Navigator() => Api.Navigator.Create(JsRuntime);

        public IMediaStream MediaStream() =>   Api.MediaStream.Create(JsRuntime);

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration) =>
            Api.RTCPeerConnection.Create(JsRuntime, configuration);


        //// TODO: REFACTOR WHOLE BLAZOR API BY USING System.Private.Runtime.InteropServices.JavaScript and use HostObject.
        ////public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null)
        ////{
        ////return new MediaRecorder(stream, options);
        ////}
        ///

        public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null) =>
            Api.MediaRecorder.Create(JsRuntime, stream, options);

    }
}
