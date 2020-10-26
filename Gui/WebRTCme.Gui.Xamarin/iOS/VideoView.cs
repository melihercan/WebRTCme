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


        private readonly VideoOptions _videoOptions;
        private readonly VideoWebRtc _videoWebRtc;
        private AVCaptureVideoPreviewLayer _previewLayer;


        public VideoView(VideoOptions videoOptions)
        {
            _videoOptions = videoOptions;
            _videoWebRtc = new VideoWebRtc();




            ///LocalVideoView = new RTCCameraPreviewView();

            CaptureSession = new AVCaptureSession();
            _previewLayer = new AVCaptureVideoPreviewLayer(CaptureSession)
            {
                Frame = Bounds,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };

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
            Layer.AddSublayer(_previewLayer);
            CaptureSession.StartRunning();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (_previewLayer != null)
            {
                _previewLayer.Frame = Bounds;
            }
        }


    }
}
