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
    internal class ApiExtensions : ApiBase, IApiExtensions
    {
        public static IApiExtensions Create() => new ApiExtensions();

        private ApiExtensions() { }


        public void SetCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType, MediaStreamConstraints mediaStreamConstraints = null)
        {
            throw new NotImplementedException();
        }


    }
}
