using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        public void SetCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType, 
            MediaStreamConstraints mediaStreamConstraints)
        {
            if (cameraType == CameraType.Default)
                cameraType = CameraType.Front;

            var context = Xamarin.Essentials.Platform.CurrentActivity.ApplicationContext;
            var nativeTrack = cameraVideoTrack.NativeObject as Webrtc.VideoTrack;
            var eglBaseContext = EglBaseHelper.Create().EglBaseContext;
            var videoSource = cameraVideoTrack.GetNativeMediaSource() as Webrtc.VideoSource;

            var cameraEnumerator = new Webrtc.Camera2Enumerator(context);
            var videoCapturer = cameraEnumerator.CreateCapturer(cameraVideoTrack.Id, null);
            videoCapturer.Initialize(Webrtc.SurfaceTextureHelper.Create(
                "CameraVideoCapturerThread",
                eglBaseContext),
                context,
                videoSource.CapturerObserver);
            videoCapturer.StartCapture(480, 640, 30);
        }
    }
}
