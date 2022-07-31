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

namespace WebRTCme.Middleware
{
    public class MediaView : ViewGroup
    {

        private readonly Context _context;
        private readonly Webrtc.SurfaceViewRenderer _rendererView;
        private readonly Webrtc.IEglBaseContext _eglBaseContext;
        //private bool _isCamera;

        public MediaView(Context context) : base(context)
        {
            _context = context;
            _eglBaseContext = AndroidSupport.GetNativeEglBase().EglBaseContext;

            _rendererView = new Webrtc.SurfaceViewRenderer(context);
            _rendererView.SetMirror(false);
            _rendererView.SetEnableHardwareScaler(true);
            _rendererView.Init(_eglBaseContext, null);
            AddView(_rendererView);
        }

        public void SetTrack(IMediaStreamTrack videoTrack)
        {
            AndroidSupport.SetTrack(videoTrack, _rendererView, _context/*, _eglBaseContext*/);
            //var nativeVideoTrack = videoTrack.NativeObject as Webrtc.VideoTrack;

            //var cameraEnum = new Webrtc.Camera2Enumerator(_context);
            //var cameraDevices = cameraEnum.GetDeviceNames(); 
            //var isCamera = cameraDevices.Any(device => device == videoTrack.Id);

            //if (isCamera)
            //{
            //    var nativeVideoSource = AndroidSupport.GetNativeVideoSource(videoTrack);
            //    var videoCapturer = cameraEnum.CreateCapturer(videoTrack.Id, null);
            //    videoCapturer.Initialize(
            //        Webrtc.SurfaceTextureHelper.Create(
            //            "CameraVideoCapturerThread",
            //            _eglBaseContext),
            //        _context,
            //        nativeVideoSource.CapturerObserver);
            //    videoCapturer.StartCapture(480, 640, 30);
            //}

            //nativeVideoTrack.AddSink(_rendererView);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            System.Diagnostics.Debug.WriteLine($"@@@@@@ OnLayout {changed}, {l} , {t}, {r}, {b}");
            _rendererView.Layout(l, t, r, b);
        }
    }
}


