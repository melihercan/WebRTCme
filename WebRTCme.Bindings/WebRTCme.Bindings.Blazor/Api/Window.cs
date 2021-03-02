using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsBlazor.Extensions;
using WebRtcBindingsBlazor.Interops;
using WebRTCme;

namespace WebRtcBindingsBlazor.Api
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

    }
}
