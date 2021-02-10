using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme;
using WebRTCme.Middleware;
using Webrtc = Org.Webrtc;

namespace WebRtcMiddlewareXamarin
{
    public class MediaView : ViewGroup
    {

        private readonly Context _context;
        private readonly IApiExtensions _apiExtensions;

        private bool _isCamera;
        private readonly Webrtc.SurfaceViewRenderer rendererView;
        private Webrtc.IEglBaseContext eglBaseContext;


        public MediaView(Context context) : base(context)
        {
            _context = context;

            _apiExtensions = WebRtcMiddleware.WebRtc.Window().ApiExtensions();
            eglBaseContext = _apiExtensions.GetEglBaseContext().NativeObject as Webrtc.IEglBaseContext;

            rendererView = new Webrtc.SurfaceViewRenderer(context);
            rendererView.SetMirror(false);
            rendererView.SetEnableHardwareScaler(true);
            rendererView.Init(eglBaseContext, null);
            AddView(rendererView);
        }

        public void SetTrack(IMediaStreamTrack videoTrack)
        {
            var _nativeTrack = videoTrack.NativeObject as Webrtc.VideoTrack;

            var cameraEnum = new Webrtc.Camera2Enumerator(_context);
            var cameraDevices = cameraEnum.GetDeviceNames(); 
            _isCamera = cameraDevices.Any(device => device == videoTrack.Id);

            if (_isCamera)
            {
                var videoSource = PlatformSupport.GetNativeVideoSource(videoTrack);// videoTrack.GetNativeMediaSource() as Webrtc.VideoSource;

                //var deviceNames = cameraEnum.GetDeviceNames();
                //var cameraName = deviceNames.First(dn => Xamarin.Essentials.DeviceInfo.DeviceType == 
                //        Xamarin.Essentials.DeviceType.Virtual
                //    ? cameraEnum.IsBackFacing(dn)
                //    : cameraEnum.IsFrontFacing(dn));
                var videoCapturer = cameraEnum.CreateCapturer(/*cameraName*/videoTrack.Id, null);
                videoCapturer.Initialize(
                    Webrtc.SurfaceTextureHelper.Create(
                        "CameraVideoCapturerThread",
                        eglBaseContext),
                    _context,
                    videoSource.CapturerObserver);
                videoCapturer.StartCapture(480, 640, 30);
            }




            _nativeTrack.AddSink(rendererView);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            rendererView.Layout(l, t, r, b);
        }
    }
}


