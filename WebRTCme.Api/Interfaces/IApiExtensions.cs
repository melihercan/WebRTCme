using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IApiExtensions
    {
        void SetCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType = CameraType.Default,
            MediaStreamConstraints mediaStreamConstraints = null);


    }
}
