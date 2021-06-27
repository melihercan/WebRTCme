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
    internal class LocalMediaStream : ILocalMediaStream
    {
        private readonly IMediaDevices _mediaDevices;

        public LocalMediaStream(IWebRtcMiddleware webRtcMiddleware, IJSRuntime jsRuntime = null)
        {
            var window = webRtcMiddleware.WebRtc.Window(jsRuntime);
            var navigator = window.Navigator();
            _mediaDevices = navigator.MediaDevices;
        }

        public async Task<IMediaStream> GetCameraMediaStreamAsync(CameraType cameraType = CameraType.Default,
            MediaStreamConstraints mediaStreamConstraints = null)
        {
            var mediaStream = await _mediaDevices.GetUserMedia(mediaStreamConstraints ?? new MediaStreamConstraints
            {
                Audio = new MediaStreamContraintsUnion { Value = true },
                Video = new MediaStreamContraintsUnion { Value = true }
            });
            return mediaStream;
        }

        public async Task<IMediaStream> GetDisplayMediaStreamAync(MediaStreamConstraints mediaStreamConstraints = null)
        {
            var mediaStream = await _mediaDevices.GetDisplayMedia(mediaStreamConstraints ?? new MediaStreamConstraints
            { 
                Video = new MediaStreamContraintsUnion
                {
                    Object = new MediaTrackConstraints
                    {
                        Cursor = CursorOptions.Never,
                        DisplaySurface = DisplaySurfaceOptions.Monitor
                    }
                }
            });
            return mediaStream;
        }
    }
}
