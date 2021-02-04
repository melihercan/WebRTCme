using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;
using Xamarin.Essentials;

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
            var permission = await Permissions.RequestAsync<Permissions.Camera>();
            if (permission != PermissionStatus.Granted)
            {
                throw new Exception("No Video permission was granted");
            }
////            var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();


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
