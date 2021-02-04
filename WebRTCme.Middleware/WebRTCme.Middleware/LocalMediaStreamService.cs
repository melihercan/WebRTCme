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
    internal class LocalMediaStreamService : ILocalMediaStreamService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IWindow _window;
        private readonly IApiExtensions _apiExtensions;

        static public ILocalMediaStreamService Create(IJSRuntime jsRuntime = null)
        {
            var self = new LocalMediaStreamService(jsRuntime);
            return self;
        }

        private LocalMediaStreamService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _window = WebRtcMiddleware.WebRtc.Window();
            _apiExtensions = _window.ApiExtensions();

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
            _apiExtensions.SetCameraVideoCapturer(mediaStream.GetVideoTracks().Single());
            return mediaStream;
        }

    }
}
