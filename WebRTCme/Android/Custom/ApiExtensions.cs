using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtc.Android;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class ApiExtensions : ApiBase, IApiExtensions
    {
        public static IApiExtensions Create() => new ApiExtensions();

        private ApiExtensions() 
        {
        }


        public IVideoCapturer SetCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType, 
            MediaStreamConstraints mediaStreamConstraints)
        {
            if (cameraType == CameraType.Default)
                cameraType = CameraType.Front;

            var context = Xamarin.Essentials.Platform.CurrentActivity.ApplicationContext;
            var nativeTrack = cameraVideoTrack.NativeObject as Webrtc.VideoTrack;
            var eglBaseContext = WebRTCme.WebRtc.NativeEglBase.EglBaseContext;// EglBaseHelper.Create().EglBaseContext;
            var videoSource = cameraVideoTrack.GetNativeMediaSource() as Webrtc.VideoSource;

            var cameraEnum = new Webrtc.Camera2Enumerator(context);
            //var deviceNames = cameraEnum.GetDeviceNames();
            //var cameraName = deviceNames.First(dn => Xamarin.Essentials.DeviceInfo.DeviceType == 
            //        Xamarin.Essentials.DeviceType.Virtual
            //    ? cameraEnum.IsBackFacing(dn)
            //    : cameraEnum.IsFrontFacing(dn));
            var videoCapturer = cameraEnum.CreateCapturer(/*cameraName*/cameraVideoTrack.Id, null);
            videoCapturer.Initialize(
                Webrtc.SurfaceTextureHelper.Create(
                    "CameraVideoCapturerThread",
                    eglBaseContext),
                context,
                videoSource.CapturerObserver);
            videoCapturer.StartCapture(480, 640, 30);
            return VideoCapturer.Create(videoCapturer);
        }

        public IEglBaseContext GetEglBaseContext() =>
             EglBaseContext.Create(WebRTCme.WebRtc.NativeEglBase.EglBaseContext);
    }
}
