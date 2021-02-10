using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Middleware
{
    public interface IMediaStreamService
    {
        Task SetCameraMediaStreamPermissionsAsync();

        Task<IMediaStream> GetCameraMediaStreamAsync(CameraType cameraType = CameraType.Default,
            MediaStreamConstraints mediaStreamConstraints = null);

        void SetCameraMediaStreamCapturer(IMediaStream mediaStream);
    }
}
