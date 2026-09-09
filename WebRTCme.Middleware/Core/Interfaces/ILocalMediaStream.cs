using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Middleware
{
    public interface ILocalMediaStream
    {
        Task<IMediaStream> GetCameraMediaStreamAsync(CameraType cameraType = CameraType.Default,
            MediaStreamConstraints mediaStreamConstraints = null);

        Task<IMediaStream> GetDisplayMediaStreamAync(MediaStreamConstraints mediaStreamConstraints = null);
    }
}
