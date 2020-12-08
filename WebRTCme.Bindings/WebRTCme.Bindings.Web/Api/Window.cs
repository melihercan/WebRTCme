using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Extensions;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    public class Window : ApiBase, IWindow
    {
        public static IWindow Create(IJSRuntime jsRuntime) =>
            new Window(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", null));

        private Window(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public INavigator Navigator() => Api.Navigator.Create(JsRuntime);

        public IRTCConfiguration RTCConfiguration() =>
            Api.RTCConfiguration.Create(JsRuntime, JsRuntime.CreateJsObject(NativeObject, "RTCConfiguration"));

        public IRTCPeerConnection RTCPeerConnection(IRTCConfiguration configuration) =>
            Api.RTCPeerConnection.Create(JsRuntime, configuration);


    }
}
