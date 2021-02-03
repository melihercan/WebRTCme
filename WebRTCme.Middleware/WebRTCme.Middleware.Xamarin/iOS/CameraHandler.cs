using CoreGraphics;
using CoreMedia;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using WebRTCme;
using System.Linq;
using AVFoundation;

namespace WebRtcMiddlewareXamarin
{
    public class CameraHandler : NSObject, Webrtc.IRTCVideoViewDelegate
    {
        private readonly IMediaStreamTrack _track;
        private readonly Webrtc.RTCVideoTrack _nativeTrack; 

        public CameraHandler(IMediaStreamTrack track)
        {
            _track = track;
            _nativeTrack = track.NativeObject as Webrtc.RTCVideoTrack;
        }

        public UIView _localView;
        public Webrtc.RTCCameraVideoCapturer _videoCapturer;
        public Webrtc.RTCVideoTrack _localVideoTrack;
        public Webrtc.RTCEAGLVideoView _localRenderView;

        CGSize _capturerSize = CGSize.Empty;
        CGSize _rendererSize = CGSize.Empty;

        private Webrtc.RTCPeerConnectionFactory _peerConnectionFactory;


#if false
        public void Xxx()
        {
            //var videoEncoderFactory = new Webrtc.RTCDefaultVideoEncoderFactory();
            //var videoDecoderFactory = new Webrtc.RTCDefaultVideoDecoderFactory();
            //_peerConnectionFactory = new Webrtc.RTCPeerConnectionFactory(videoEncoderFactory, videoDecoderFactory);

            _localRenderView = new Webrtc.RTCEAGLVideoView();
            _localRenderView.Delegate = this;
            //_localView = new UIView();
            //_localView.BackgroundColor = UIColor.SystemBlueColor;
            //_localView.AddSubview(_localRenderView);


            var videoSource = _nativeTrack.Source;// _peerConnectionFactory.VideoSource;

            _videoCapturer = new Webrtc.RTCCameraVideoCapturer();
            _videoCapturer.Delegate = videoSource;

            _localVideoTrack = _nativeTrack;// _peerConnectionFactory.VideoTrackWithSource(videoSource, "video0");


            var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Front);

            var formats = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device);
            System.Diagnostics.Debug.WriteLine($"============= Capture Formats =============== ");
            int index = 0;
            foreach (var f in formats)
            {
                CMVideoFormatDescription desc = (CMVideoFormatDescription)f.FormatDescription;
                var dim = desc.Dimensions;
                var maxSupportedFps = 0d;
                foreach (var fpsRange in f.VideoSupportedFrameRateRanges)
                    maxSupportedFps = Math.Max(maxSupportedFps, fpsRange.MaxFrameRate);
                System.Diagnostics.Debug.WriteLine($"index:{index++} width:{dim.Width} height:{dim.Height} fpsMax:{maxSupportedFps}");
            }


            var format = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
            CMVideoFormatDescription videoFormatDescription = (CMVideoFormatDescription)format.FormatDescription;
            var capturerDimensions = videoFormatDescription.Dimensions;
            _capturerSize = new CGSize(capturerDimensions.Width, capturerDimensions.Height);
            var fps = 30;
            _videoCapturer.StartCaptureWithDevice(device, format, fps);


            _localVideoTrack.AddRenderer(_localRenderView);


        }
#endif

        public void Yyy()
        {

//            _localRenderView = new Webrtc.RTCEAGLVideoView();
  //          _localRenderView.Delegate = this;


            var videoSource = _nativeTrack.Source;// _peerConnectionFactory.VideoSource;

            _videoCapturer = new Webrtc.RTCCameraVideoCapturer();
            _videoCapturer.Delegate = videoSource;

            //_localVideoTrack = _nativeTrack;// _peerConnectionFactory.VideoTrackWithSource(videoSource, "video0");


            var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Front);

            var formats = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device);
            System.Diagnostics.Debug.WriteLine($"============= Capture Formats =============== ");
            int index = 0;
            foreach (var f in formats)
            {
                CMVideoFormatDescription desc = (CMVideoFormatDescription)f.FormatDescription;
                var dim = desc.Dimensions;
                var maxSupportedFps = 0d;
                foreach (var fpsRange in f.VideoSupportedFrameRateRanges)
                    maxSupportedFps = Math.Max(maxSupportedFps, fpsRange.MaxFrameRate);
                System.Diagnostics.Debug.WriteLine($"index:{index++} width:{dim.Width} height:{dim.Height} fpsMax:{maxSupportedFps}");
            }


            var format = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
            CMVideoFormatDescription videoFormatDescription = (CMVideoFormatDescription)format.FormatDescription;
            var capturerDimensions = videoFormatDescription.Dimensions;
            _capturerSize = new CGSize(capturerDimensions.Width, capturerDimensions.Height);
            var fps = 30;
            _videoCapturer.StartCaptureWithDevice(device, format, fps);


    //        _localVideoTrack.AddRenderer(_localRenderView);


        }

        public void Zzz()
        {
             _localRenderView = new Webrtc.RTCEAGLVideoView();
            _localRenderView.Delegate = this;
            //_nativeTrack = VideoTrack.NativeObject as Webrtc.RTCVideoTrack;
            _nativeTrack.AddRenderer(_localRenderView);
        }


#if true
        [Export("videoView:didChangeVideoSize:")]
        public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        {
            if (videoView is Webrtc.RTCEAGLVideoView renderer)
            {
                System.Diagnostics.Debug.WriteLine($"@@@@@@ DidChangeVideoSize renderer.Frame:{renderer.Frame} size:{size}");

                _rendererSize = size;
            }
        }
#endif
    }
}
