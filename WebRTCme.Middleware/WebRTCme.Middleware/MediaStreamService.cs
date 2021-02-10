using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRtcMeMiddleware
{
    internal class MediaStreamService : IMediaStreamService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IWindow _window;


        static public IMediaStreamService Create(IJSRuntime jsRuntime = null)
        {
            var self = new MediaStreamService(jsRuntime);
            return self;
        }


        private MediaStreamService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _window = WebRtcMiddleware.WebRtc.Window(jsRuntime);
        }

        public async Task<IMediaStream> GetCameraMediaStreamAsync(CameraType cameraType = CameraType.Default,
            MediaStreamConstraints mediaStreamConstraints = null)
        {
            var navigator = _window.Navigator();
            var mediaDevices = navigator.MediaDevices;
            var mediaStream = await mediaDevices.GetUserMedia(mediaStreamConstraints ?? new MediaStreamConstraints
            {
                Audio = new MediaStreamContraintsUnion { Value = true },
                Video = new MediaStreamContraintsUnion { Value = true }
            });
            return mediaStream;
        }
    }
}
