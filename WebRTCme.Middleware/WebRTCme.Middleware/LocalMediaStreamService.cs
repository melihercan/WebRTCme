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
        private IJSRuntime _jsRuntime;
        private IPlatform _platform;

        static public ILocalMediaStreamService Create(IJSRuntime jsRuntime = null)
        {
            var self = new LocalMediaStreamService();
            self._jsRuntime = jsRuntime;
            self._platform = WebRtcMiddleware.WebRtc.Window().Platform();
            return self;
        }

        private LocalMediaStreamService()
        {
        }

        public async Task<IMediaStream> GetCameraMediaStreamAsync(CameraType cameraType = CameraType.Default, 
            MediaStreamConstraints mediaStreamConstraints = null)
        {
            var window = WebRtcMiddleware.WebRtc.Window();
            var navigator = window.Navigator();
            var mediaDevices = navigator.MediaDevices;
            var mediaStream = await mediaDevices.GetUserMedia(mediaStreamConstraints ?? new MediaStreamConstraints
            {
                Audio = new MediaStreamContraintsUnion { Value = true },
                Video = new MediaStreamContraintsUnion { Value = true }
            });

            //var platform = window.Platform();
            //platform.SetCameraCapturer(mediaStream.GetVideoTracks().Single());

            return mediaStream;
        }


        public async Task<IMediaStreamTrack> GetCameraTrackAsync(MediaStreamConstraints mediaStreamConstraints = null)
        {
            var stream = await GetCameraMediaStreamAsync();
            return stream.GetVideoTracks().Single();
        }

        public IVideoView GetCameraView() => _platform.GetCameraView();

        public IVideoRenderer GetCameraRenderer() => _platform.GetCameraRenderer();

        public IVideoCapturer GetCameraCapturer() => _platform.GetCameraCapturer();
    }
}
