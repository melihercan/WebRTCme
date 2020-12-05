using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Extensions;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        public static IMediaDevices Create(IJSRuntime jsRuntime) =>
            new MediaDevices(jsRuntime, jsRuntime.GetJsPropertyObjectRef("window", "navigator.mediaDevices"));

        private MediaDevices(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public async Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints)
        {
            var jsObjectRefMediaStream = await JsRuntime.CallJsMethodAsync<JsObjectRef>(NativeObject, 
                "getUserMedia", constraints);
            return MediaStream.Create(JsRuntime, jsObjectRefMediaStream);
        }

        public async Task<IEnumerable<MediaDeviceInfo>> EnumerateDevices()
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
            return mediaDeviceInfos;
        }


        public MediaTrackSupportedConstraints GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }
    }
}
