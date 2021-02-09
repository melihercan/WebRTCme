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
    public class RendererViewProxy : NSObject, Webrtc.IRTCVideoViewDelegate
    {
        private readonly IMediaStreamTrack _track;
        private readonly Webrtc.RTCVideoTrack _nativeTrack;


#if false
        public RendererViewProxy()
        {
            var videoEncoderFactory = new Webrtc.RTCDefaultVideoEncoderFactory();
            var videoDecoderFactory = new Webrtc.RTCDefaultVideoDecoderFactory();
            var _peerConnectionFactory = new Webrtc.RTCPeerConnectionFactory(videoEncoderFactory, videoDecoderFactory);

            var videoSource = _peerConnectionFactory.VideoSource;
            var _videoCapturer = new Webrtc.RTCCameraVideoCapturer();
            _videoCapturer.Delegate = videoSource;
            var _localVideoTrack = _peerConnectionFactory.VideoTrackWithSource(videoSource, "video0");


            var position = AVCaptureDevicePosition/*.Front;*/.Back;
            var width = 640;//1280;// 352;// 640;
            var height = 320;//720;// 288;// 480;
            int fps = 30;


            var devices = Webrtc.RTCCameraVideoCapturer.CaptureDevices;
            var targetDevice = devices.FirstOrDefault(d => d.Position == position);

            if (targetDevice != null)
            {
                var formats = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(targetDevice);

                var targetFormat = formats.FirstOrDefault(f =>
                {
                    var description = f.FormatDescription;
                    if (description is CMVideoFormatDescription videoDescription)
                    {
                        var dimensions = videoDescription.Dimensions;
                        if ((dimensions.Width == width && dimensions.Height == height) ||
                            (dimensions.Width == width))
                        {
                            return true;
                        }
                    }

                    return false;
                });

                if (targetFormat != null)
                {
                    CMVideoFormatDescription videoFormatDescription = (CMVideoFormatDescription)targetFormat.FormatDescription;
                    var capturerDimensions = videoFormatDescription.Dimensions;
                    var _capturerSize = new CGSize(capturerDimensions.Width, capturerDimensions.Height);

                    _videoCapturer.StartCaptureWithDevice(targetDevice, targetFormat, fps);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("????????????? targetFormat not found");
                    throw new Exception("targetFormat not found");
                }

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("????????????? targetDevice not found");
                throw new Exception("targetDevice not found");
            }

            _nativeTrack = _localVideoTrack;

        }
#endif


        public RendererViewProxy(IMediaStreamTrack track)
        {
            _track = track;
            _nativeTrack = track.NativeObject as Webrtc.RTCVideoTrack;
        }

        public Webrtc.RTCEAGLVideoView RendererView
        {
            get
            {
                var rendererView = new Webrtc.RTCEAGLVideoView();
                rendererView.Delegate = this;
                _nativeTrack.AddRenderer(rendererView);
                return rendererView;
            }
        }

        [Export("videoView:didChangeVideoSize:")]
        public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        {
            if (videoView is Webrtc.RTCEAGLVideoView renderer && renderer.Superview is UIView parent)
            {
                System.Diagnostics.Debug.WriteLine($"@@@@@@ DidChangeVideoSize renderer.Frame:{renderer.Frame} " +
                    $"size:{size}");
//                parent.Frame = new CGRect(0, 0, size.Width, size.Height);
  //              parent.SetNeedsLayout();
            }
        }
    }
}
