using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRTCme.Middleware.Services
{
    internal class MediaStreamService : IMediaStreamService
    {
        private readonly IWindow _window;


        public MediaStreamService(IJSRuntime jsRuntime = null)
        {
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
