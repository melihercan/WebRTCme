using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRtc.Android;
using WebRTCme;
using Xamarin.Essentials;
using Webrtc = Org.Webrtc;

namespace WebRTCme
{
    public static class PlatformSupport
    {
        public static SurfaceView CreateCameraView(IMediaStreamTrack track, MediaTrackConstraints constraints = null)
        {
            var context = Platform.CurrentActivity.ApplicationContext;
            var nativeVideoTrack = track.NativeObject as Webrtc.VideoTrack;

            var eglBaseContext = EglBaseHelper.Create().EglBaseContext;
            var nativeCameraEnumerator = new Webrtc.Camera2Enumerator(context);
            var nativeCameraVideoCapturer = nativeCameraEnumerator.CreateCapturer(track.Id, null);

            var nativeVideoSource = track.GetNativeMediaSource() as Webrtc.VideoSource;
            nativeCameraVideoCapturer.Initialize(Webrtc.SurfaceTextureHelper.Create(
                "CameraVideoCapturerThread",
                eglBaseContext),
                context,
                nativeVideoSource.CapturerObserver);
            nativeCameraVideoCapturer.StartCapture(480, 640, 30);

            var nativeSurfaceViewRenderer = new Webrtc.SurfaceViewRenderer(context);
            nativeSurfaceViewRenderer.SetMirror(true);
            nativeSurfaceViewRenderer.Init(eglBaseContext, null);

            nativeVideoTrack.AddSink(nativeSurfaceViewRenderer);
            return nativeSurfaceViewRenderer;
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

