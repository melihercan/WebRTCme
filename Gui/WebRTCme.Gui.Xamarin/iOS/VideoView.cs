using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Webrtc;

namespace WebRtcGuiXamarin.iOS
{
    public class VideoView : UIView
    {

        public /****UIView***/RTCCameraPreviewView LocalVideoView { get; }
        ////        public UIView RemoteVideoView { get; }

        public AVCaptureSession CaptureSession { get; private set; }


        private readonly Video.TypeEnum _type;
        private readonly string _source;
        private readonly VideoWebRtc _videoWebRtc;
        private AVCaptureVideoPreviewLayer _previewLayer;


        public VideoView(Video.TypeEnum type, string sourceIn)
        {
            _type = type;
            _source = sourceIn;
            _videoWebRtc = new VideoWebRtc();


            var videoDecoderFactory = new RTCDefaultVideoDecoderFactory();
            var videoEncoderFactory = new RTCDefaultVideoEncoderFactory();
            var peerConnectionFactory = new RTCPeerConnectionFactory(videoEncoderFactory, videoDecoderFactory);
            var source = peerConnectionFactory.VideoSource;




            LocalVideoView = new RTCCameraPreviewView();
            AddSubview(LocalVideoView);
            var cameraVideoCapturer = new RTCCameraVideoCapturer(source);
            LocalVideoView.CaptureSession = cameraVideoCapturer.CaptureSession;

        var videoTrack = peerConnectionFactory.VideoTrackWithSource(source, "MyVideoTrack");
        var mediaStream = peerConnectionFactory.MediaStreamWithStreamId("MyMediaStream");


            var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            var cameraPosition = AVCaptureDevicePosition.Front;// AVCaptureDevicePosition.Back;
            var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);
            var format = RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
            var fps = GetFpsByFormat(format);
            cameraVideoCapturer.StartCaptureWithDevice(device, format, fps);


            int GetFpsByFormat(AVCaptureDeviceFormat fmt)
            {
                const float _frameRateLimit = 30.0f;

                var maxSupportedFps = 0d;
                foreach (var fpsRange in fmt.VideoSupportedFrameRateRanges) 
                    maxSupportedFps = Math.Max(maxSupportedFps, fpsRange.MaxFrameRate);

                return (int)Math.Min(maxSupportedFps, _frameRateLimit);
            }


#if false

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
