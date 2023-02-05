using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class MediaDevices : NativeBase, IMediaDevices
    {
        public MediaDevices(IJSRuntime jsRuntime) : 
            this(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", "navigator.mediaDevices"))
        {
        }

        public MediaDevices(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListenerForObjectRef("devicechange", (s, e) => OnDeviceChange?.Invoke(s, e), 
                MediaStreamTrackEvent.Create);
        }

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public async Task<MediaDeviceInfo[]> EnumerateDevices()
        {
            var mediaDeviceInfos = new List<MediaDeviceInfo>();
            var jsObjectRef = await JsRuntime.CallJsMethodAsync<JsObjectRef>(NativeObject, "enumerateDevices");
            var jsObjectRefMediaDeviceInfoArray = JsRuntime.GetJsPropertyArray(jsObjectRef);
            foreach (var jsObjectRefMediaDeviceInfo in jsObjectRefMediaDeviceInfoArray)
            {
                mediaDeviceInfos.Add(JsRuntime.GetJsPropertyValue<MediaDeviceInfo>
                    (jsObjectRefMediaDeviceInfo, null));
                JsRuntime.DeleteJsObjectRef(jsObjectRefMediaDeviceInfo.JsObjectRefId);
            }
            JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            return mediaDeviceInfos.ToArray();
        }

        public MediaTrackSupportedConstraints GetSupportedConstraints() =>
            GetNativeProperty<MediaTrackSupportedConstraints>("getSupportedConstraints");

        public async Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints) =>
            await Task.FromResult(new MediaStream(JsRuntime, 
                await JsRuntime.CallJsMethodAsync<JsObjectRef>(NativeObject, "getDisplayMedia")));

        public async Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints) =>
            await Task.FromResult(new MediaStream(JsRuntime,
                await JsRuntime.CallJsMethodAsync<JsObjectRef>(NativeObject, "getUserMedia", constraints)));
    }
}
