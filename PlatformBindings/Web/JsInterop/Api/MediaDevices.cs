using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        private MediaDevices(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public async Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints)
        {
            var jsObjectRefMediaStream = await JsRuntime.CallJsMethodAsync<JsObjectRef>(SelfNativeObject, 
                "getUserMedia", constraints);
            return MediaStream.Create(JsRuntime, jsObjectRefMediaStream);
        }

        public async Task<IEnumerable<MediaDeviceInfo>> EnumerateDevices()
        {
            var mediaDeviceInfos = new List<MediaDeviceInfo>();
            var jsObjectRef = await JsRuntime.CallJsMethodAsync<JsObjectRef>(SelfNativeObject, "enumerateDevices");
            var jsObjectRefMediaDeviceInfoArray = await JsRuntime.GetJsPropertyArray(jsObjectRef);
            foreach (var jsObjectRefMediaDeviceInfo in jsObjectRefMediaDeviceInfoArray)
            {
                mediaDeviceInfos.Add(await JsRuntime.GetJsPropertyValue<MediaDeviceInfo>
                    (jsObjectRefMediaDeviceInfo, null, null));
                await JsRuntime.DeleteJsObjectRef(jsObjectRefMediaDeviceInfo.JsObjectRefId);
            }
            await JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            return mediaDeviceInfos;
        }

        internal static async Task<IMediaDevices> CreateAsync(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetJsPropertyObjectRef("window", "navigator.mediaDevices");
            var mediaDevices = new MediaDevices(jsRuntime, jsObjectRef);
            return mediaDevices;
        }

        public Task<MediaTrackSupportedConstraints> GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }
    }
}
