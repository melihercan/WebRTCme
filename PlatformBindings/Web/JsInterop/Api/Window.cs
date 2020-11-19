using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    public class Window : ApiBase, IWindow
    {
        private Window(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public async Task<INavigator> Navigator() => await Api.Navigator.CreateAsync(JsRuntime);

        public async Task<IRTCPeerConnection> RTCPeerConnection(RTCConfiguration rtcConfiguration) =>
            await Api.RTCPeerConnection.CreateAsync(JsRuntime, rtcConfiguration);

        public static async Task<IWindow> CreateAsync(IJSRuntime jsRuntime)
        {
            //var windowObjRef = await jsRuntime.InvokeAsync<JsObjectRef>(
            //    "webRtcInterop.getProperty", null, "window");
            // windowObjRef.JsRuntime = jsRuntime;

            //var navigatorObjRef = await jsRuntime.InvokeAsync<JsObjectRef>(
            //    "webRtcInterop.getProperty", windowObjRef, "navigator");
            //navigatorObjRef.JsRuntime = jsRuntime;

            //var mediaDevicesObjRef = await jsRuntime.InvokeAsync<JsObjectRef>(
            //    "webRtcInterop.getProperty", navigatorObjRef, "mediaDevices");
            //mediaDevicesObjRef.JsRuntime = jsRuntime;

            //var mediaDevicesObjRef2 = await jsRuntime.InvokeAsync<JsObjectRef>(
            //    "webRtcInterop.getProperty", 
            //    new object[]
            //    {
            //        windowObjRef,
            //        "navigator.mediaDevices"
            //    });
            //mediaDevicesObjRef2.JsRuntime = jsRuntime;

            //var rtcPeerConnectionObjRef = await jsRuntime.InvokeAsync<JsObjectRef>(
            //    "webRtcInterop.createObject",
            //    new object[]
            //    {
            //        null,
            //        "RTCPeerConnection"
            //    });
            //rtcPeerConnectionObjRef.JsRuntime = jsRuntime;

            //object contentSpec = new
            //{
            //    closed = true,
            //    innerHeight = true,
            //    innerWidth = true,
            //    isSecureContext = true,
            //    name = true,
            //    origin = true,
            //    outerHeight = true,
            //    outerWidth = true,
            //    screenX = true,
            //    screenY = true,
            //    scrollX = true,
            //    scrollY = true
            //};
            //var content = await jsRuntime.InvokeAsync<WindowInterop>(
            //    "webRtcInterop.getContent",
            //    new object[]
            //    {
            //        null,
            //        "window",
            //        contentSpec
            //    });


            //var streamObjRef = await jsRuntime.InvokeAsync<JsObjectRef>(
            //    "webRtcInterop.callMethodAsync",
            //    new object[]
            //    {
            //        mediaDevicesObjRef,
            //        "getUserMedia",
            //        new MediaStreamConstraints
            //        {
            //            Audio = true,
            //            Video = true
            //        }
            //    });

            //var obj = await jsRuntime.InvokeAsync<object>(
            //    "webRtcInterop.callMethod",
            //    new object[]
            //    {
            //        windowObjRef,
            //        "alert",
            //        "hello melik"
            //    });


            //await mediaDevicesObjRef2.DisposeAsync();
            //await mediaDevicesObjRef.DisposeAsync();
            //await navigatorObjRef.DisposeAsync();
            //await windowObjRef.DisposeAsync();



            //            var windowInterop = await jsRuntime.Window();
            //          var window = new Window(jsRuntime, windowInterop);

            var jsObjectRef = await jsRuntime.GetJsPropertyObjectRef("window", null);
            var window = new Window(jsRuntime, jsObjectRef);
            return window;
        }
    }
}
