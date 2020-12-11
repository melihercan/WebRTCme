using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme;

namespace WebRTCme
{
    public static class PlatformSupport
    {
        public static View CreateCameraView(IMediaStreamTrack track, MediaTrackConstraints constraints = null)
        {
            return null;
        }
    }
}

///// !!!!AddRenderer will not work with RTCCameraPreviewView as it is no derived from RTCVideoRenderer
/// !!!! SO USE IT WITH REMOTE 
//var renderer = (UIView)_nativeCameraPreviewView;    ////????
//((RTCVideoTrack)NativeObject).AddRenderer((IRTCVideoRenderer)renderer);

/// TODO: If local, RTCCameraVideoCapturer or RTCFileVideoCapturer???
/// CURENTLY Camera is assumed.

//void AddMp4VideoStreamTrack()

//void AddRemoteVideoStreamTrack()

