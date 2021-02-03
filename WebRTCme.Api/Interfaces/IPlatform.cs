using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IPlatform
    {
        IVideoCapturer StartCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType,
            MediaStreamConstraints mediaStreamConstraints = null);


        IVideoView GetCameraView();

        IVideoRenderer GetCameraRenderer();

        IVideoCapturer GetCameraCapturer();




    }
}
