using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using WebRTCme;

namespace WebRtcGuiXamarin.iOS
{
    public class VideoView : UIView
    {

        public UIView /****RTCCameraPreviewView****/ LocalVideoView { get; }
        ////        public UIView RemoteVideoView { get; }

        public AVCaptureSession CaptureSession { get; private set; }


        ////private readonly VideoType _type;
        ////private readonly string _source;
////        private readonly VideoWebRtc _videoWebRtc;
        private AVCaptureVideoPreviewLayer _previewLayer;

        public VideoView(UIView view)
        {
            LocalVideoView = view;
            AddSubview(LocalVideoView);
        }

        public VideoView(VideoType type, string source)
        {


           //_type = type;
            //_source = source;
            ////            _videoWebRtc = new VideoWebRtc();
            ///

            //if (_type == VideoType.Local)
            //{
              //  if (string.IsNullOrEmpty(source))
                //{
                    // Default devices.
////                    var mediaDevices = WebRtcGui.WebRtc.Window.Navigator.MediaDevices;
                    ////var mediaDevicesInfo = mediaDevices.EnumerateDevices();
////                    var mediaStream = mediaDevices.GetUserMedia(new MediaStreamConstraints
////                    {
////                        Audio = new MediaStreamContraintsUnion { Value = true },
////                        Video = new MediaStreamContraintsUnion { Value = true }
////                    });



////                    var videoTrack = mediaStream.GetVideoTracks().First();
////                    LocalVideoView = videoTrack.GetView<UIView>();
////                    AddSubview(LocalVideoView);

                    ////                    videoTrack.Play<UIView>(this);

                //}
            //}


#if TESTING

            var videoDecoderFactory = new RTCDefaultVideoDecoderFactory();
            var videoEncoderFactory = new RTCDefaultVideoEncoderFactory();
            var peerConnectionFactory = new RTCPeerConnectionFactory(videoEncoderFactory, videoDecoderFactory);
            var sourceX = peerConnectionFactory.VideoSource;





            LocalVideoView = new RTCCameraPreviewView();
            AddSubview(LocalVideoView);
            var cameraVideoCapturer = new RTCCameraVideoCapturer(/****source****/);
            LocalVideoView.CaptureSession = cameraVideoCapturer.CaptureSession;

        var videoTrack = peerConnectionFactory.VideoTrackWithSource(sourceX, "MyVideoTrack");
        var mediaStream = peerConnectionFactory.MediaStreamWithStreamId("MyMediaStream");
            mediaStream.AddVideoTrack(videoTrack);

            //// Given RTCMediaStream. Get RTCVideoTrack and get capturer and stream from it.
            var vt = mediaStream.VideoTracks[0];
            var kind = vt.Kind;
            var tid = vt.TrackId;
            var ie = vt.IsEnabled;
            ////var rs = ReadyState;
            var src = vt.Source;
            var del = src.Delegate;






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
#endif

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
