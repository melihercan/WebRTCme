using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRtc.Android;
using WebRTCme;
using Xamarin.Essentials;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class Platform : ApiBase, IPlatform
    {
        public static IPlatform Create() => new Platform();

        public IVideoView GetCameraView()
        {
            throw new NotImplementedException();
        }

        public IVideoRenderer GetCameraRenderer()
        {
            throw new NotImplementedException();
        }

        public IVideoCapturer GetCameraCapturer()
        {
            throw new NotImplementedException();
        }

        public IVideoCapturer StartCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType, MediaStreamConstraints mediaStreamConstraints = null)
        {
            throw new NotImplementedException();
        }

        private Platform() { }


    }
}
