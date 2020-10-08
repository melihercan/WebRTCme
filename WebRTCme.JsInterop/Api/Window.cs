using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop
{
    public class Window : IWindow
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly WindowInterop _windowInterop;

        private Window(IJSRuntime jsRuntime, WindowInterop windowInterop)
        {
            _jsRuntime = jsRuntime;
            _windowInterop = windowInterop;
        }

        public static async Task<IWindow> New(IJSRuntime jsRuntime)
        {
            var windowObjRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "webRtcInterop.getPropertyJsObjectRef", null, "window");
            windowObjRef.JsRuntime = jsRuntime;

            var navigatorObjRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "webRtcInterop.getPropertyJsObjectRef", windowObjRef, "navigator");
            navigatorObjRef.JsRuntime = jsRuntime;

            var mediaDevicesObjRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "webRtcInterop.getPropertyJsObjectRef", navigatorObjRef, "mediaDevices");
            mediaDevicesObjRef.JsRuntime = jsRuntime;
            
            var mediaDevicesObjRef2 = await jsRuntime.InvokeAsync<JsObjectRef>(
                "webRtcInterop.getPropertyJsObjectRef", 
                new object[]
                {
                    windowObjRef,
                    "navigator.mediaDevices"
                });
            mediaDevicesObjRef2.JsRuntime = jsRuntime;

            var obj = await jsRuntime.InvokeAsync<object>(
                "webRtcInterop.callMethodAsync",
                new object[]
                {
                    mediaDevicesObjRef,
                    "getUserMedia",
                    new MediaStreamConstraints
                    {
                        Audio = true,
                        Video = true
                    }
                });


            await mediaDevicesObjRef2.DisposeAsync();
            await mediaDevicesObjRef.DisposeAsync();
            await navigatorObjRef.DisposeAsync();
            await windowObjRef.DisposeAsync();



            var windowInterop = await jsRuntime.Window();
            var window = new Window(jsRuntime, windowInterop);
            return window;
        }




        async Task<INavigator> IWindow.Navigator()
        {
            var navigatorInterop = await _windowInterop.Navigator();
            var navigator = new Navigator(_jsRuntime, navigatorInterop);
            await navigator.Init();
            return navigator;
        }

        public IRTCPeerConnection RTCPeerConnection()
        {
            return new RTCPeerConnection();
        }

    }
}
