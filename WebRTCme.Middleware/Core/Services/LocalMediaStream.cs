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

        /// <summary>
        /// Opens the camera, honouring <paramref name="cameraType"/>.
        /// </summary>
        /// <remarks>
        /// The parameter had been accepted and ignored: every call asked for <c>video: true</c>
        /// whichever camera was named, so the platform's own default answered every time. It now
        /// becomes a <c>facingMode</c> constraint, which is a request the platforms already know
        /// how to read.
        ///
        /// Explicit constraints win outright when given - they are the more specific request, and a
        /// caller writing their own video constraints has already said which camera they want.
        /// </remarks>
        public async Task<IMediaStream> GetCameraMediaStreamAsync(CameraType cameraType = CameraType.Default,
            MediaStreamConstraints mediaStreamConstraints = null)
        {
            var mediaStream = await _mediaDevices.GetUserMedia(mediaStreamConstraints ?? new MediaStreamConstraints
            {
                Audio = new MediaStreamContraintsUnion { Value = true },
                Video = VideoConstraintFor(cameraType)
            });
            return mediaStream;
        }

        /// <summary>
        /// The video constraint asking for one <see cref="CameraType"/>.
        /// </summary>
        /// <remarks>
        /// Ideal rather than exact: a device with no camera facing the requested way should still
        /// produce a call with the camera it has, which is what a bare <c>video: true</c> did
        /// before. "external" is not one of the spec's facing modes - platforms with a notion of an
        /// external camera match it, and the rest fall back to their default.
        /// </remarks>
        static MediaStreamContraintsUnion VideoConstraintFor(CameraType cameraType) =>
            cameraType switch
            {
                CameraType.Front => FacingMode("user"),
                CameraType.Back => FacingMode("environment"),
                CameraType.External => FacingMode("external"),
                _ => new MediaStreamContraintsUnion { Value = true }
            };

        static MediaStreamContraintsUnion FacingMode(string mode) => new()
        {
            Object = new MediaTrackConstraints
            {
                FacingMode = new ConstrainDOMString
                {
                    Ideal = new ConstrainDOMStringUnion { Value = mode }
                }
            }
        };

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
