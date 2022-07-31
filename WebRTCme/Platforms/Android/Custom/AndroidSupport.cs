using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme.Android;
using WebRTCme;
using Webrtc = Org.Webrtc;


namespace WebRTCme
{
    public static class AndroidSupport
    {
        public static Webrtc.VideoSource GetNativeVideoSource(IMediaStreamTrack videoTrack)
        {
            return ((MediaStreamTrack)videoTrack).GetNativeMediaSource() as Webrtc.VideoSource;
        }

        public static Webrtc.IEglBase GetNativeEglBase()
        {
            return WebRtc.NativeEglBase;
        }

        public static void SetTrack(IMediaStreamTrack videoTrack, Webrtc.SurfaceViewRenderer rendererView, 
            global::Android.Content.Context context/*, Webrtc.IEglBaseContext eglBaseContext*/)
        {
            var nativeVideoTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.VideoTrack;

            var cameraEnum = new Webrtc.Camera2Enumerator(context);
            var cameraDevices = cameraEnum.GetDeviceNames();
            var isCamera = cameraDevices.Any(device => device == videoTrack.Id);

            if (isCamera)
            {
                var nativeVideoSource = AndroidSupport.GetNativeVideoSource(videoTrack);
                var videoCapturer = cameraEnum.CreateCapturer(videoTrack.Id, null);
                videoCapturer.Initialize(
                    Webrtc.SurfaceTextureHelper.Create(
                        "CameraVideoCapturerThread",
                        GetNativeEglBase().EglBaseContext),
                    context,
                    nativeVideoSource.CapturerObserver);
                videoCapturer.StartCapture(480, 640, 30);
            }

            nativeVideoTrack.AddSink(rendererView);
        }
    }
}

