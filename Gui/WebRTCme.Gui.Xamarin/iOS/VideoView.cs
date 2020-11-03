using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Webrtc;
using Xamarin.Forms;

namespace WebRtcGuiXamarin.iOS
{
    public class VideoView : UIView
    {

        public UIView LocalVideoView { get; }
        ////        public UIView RemoteVideoView { get; }

        public AVCaptureSession CaptureSession { get; private set; }


        private readonly Video.TypeEnum _type;
        private readonly string _source;
        private readonly VideoWebRtc _videoWebRtc;
        private AVCaptureVideoPreviewLayer _previewLayer;


        public VideoView(Video.TypeEnum type, string source)
        {
            _type = type;
            _source = source;
            _videoWebRtc = new VideoWebRtc();

#if true
            CaptureSession = new AVCaptureSession();
            var cameraPreviewView = new RTCCameraPreviewView();
            cameraPreviewView.CaptureSession = CaptureSession;
            LocalVideoView = cameraPreviewView;
            AddSubview(LocalVideoView);

#endif

            ////RTCSSLAdapter.RTCInitializeSSL();

#if false
            CaptureSession = new AVCaptureSession();
                        _previewLayer = new AVCaptureVideoPreviewLayer(CaptureSession)
                        {
                            Frame = Bounds,
                            VideoGravity = AVLayerVideoGravity.ResizeAspectFill
                        };
                        Layer.AddSublayer(_previewLayer);
#endif

#if true
            var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            var cameraPosition = AVCaptureDevicePosition.Front;// AVCaptureDevicePosition.Back;
            var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);

            if (device == null)
            {
                return;
            }

            NSError error;
            var input = new AVCaptureDeviceInput(device, out error);
            CaptureSession.AddInput(input);
            CaptureSession.StartRunning();
#endif
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

#if false
            if (_previewLayer != null)
            {
                _previewLayer.Frame = Bounds;
            }
#endif
            LocalVideoView.Frame = Bounds;
        }
    }
}
