using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface ILocalMediaStreamService
    {
        Task<IMediaStream> GetCameraMediaStreamAsync(CameraType cameraType = CameraType.Default, 
            MediaStreamConstraints mediaStreamConstraints = null);


    }
}
