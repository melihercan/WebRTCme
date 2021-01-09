using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRtcMeMiddleware
{
    internal class MediaStreamService : IMediaStreamService
    {
        private IJSRuntime _jsRuntime;

        static public IMediaStreamService Create(IJSRuntime jsRuntime = null)
        {
            var self = new MediaStreamService();
            self._jsRuntime = jsRuntime;
            return self;
        }

        private MediaStreamService()
        {
        }

        public async Task<IMediaStream> GetCameraStreamAsync(string source)
        {
            var window = WebRtcMiddleware.WebRtc.Window(_jsRuntime);
            var navigator = window.Navigator();
            var mediaDevices = navigator.MediaDevices;
            var mediaDevicesInfo = await mediaDevices.EnumerateDevices();
            var mediaStream = await mediaDevices.GetUserMedia(new MediaStreamConstraints
            {
                Audio = new MediaStreamContraintsUnion { Value = true },
                Video = new MediaStreamContraintsUnion { Value = true }
            });
            return mediaStream;
        }
    }
}
