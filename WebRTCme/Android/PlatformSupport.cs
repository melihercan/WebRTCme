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
        public static View CreateCameraView(IMediaStreamTrack track, MediaTrackConstraints constraints = null)
        {
            var context = Platform.CurrentActivity.ApplicationContext;


            var nativeVideoSource = WebRtc.NativePeerConnectionFactory.CreateVideoSource(false);

            ///            var nativeCameraVideoCapturer =
            var nativeCameraVideoCapturer = new Webrtc.Camera2Capturer(context, track.Id, null);

            var eglBaseContext = EglBaseHelper.Create().EglBaseContext;


            nativeCameraVideoCapturer.Initialize(Webrtc.SurfaceTextureHelper.Create(
                "CaptureThread",
                eglBaseContext),
                context, 
                nativeVideoSource.CapturerObserver);


            nativeCameraVideoCapturer.StartCapture(100, 100, 30);


            var nativeSurfaceViewRenderer = new Webrtc.SurfaceViewRenderer(context);
            nativeSurfaceViewRenderer.Init(eglBaseContext, null);
            
            
            var nativeLocalVideoTrack = track.NativeObject as Webrtc.VideoTrack;

            var nativeVideoSink = new VideoRendererProxy();
            nativeVideoSink.Renderer = nativeSurfaceViewRenderer;

            try
            {
                nativeLocalVideoTrack.AddSink(new VideoRendererProxy());
            }
            catch(Exception ex)
            {
                var x = ex.Message;
            }








            return nativeSurfaceViewRenderer;
        }
    }

    public class VideoRendererProxy : Java.Lang.Object, Webrtc.IVideoSink
    {
        private Webrtc.IVideoSink _renderer;

////        public VideoRendererProxy()
////        {
////            _renderer = this;
////        }

////        public object NativeObject => this;

        public Webrtc.IVideoSink Renderer
        {
            get => _renderer;
            set
            {
                if (_renderer == this)
                    throw new InvalidOperationException("You can not set renderer to self");
                _renderer = value;
            }
        }

        public void OnFrame(Webrtc.VideoFrame p0)
        {
            Renderer?.OnFrame(p0);
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

