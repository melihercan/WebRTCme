using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using WebRTCme;
using CoreMedia;
using WebRTCme.Middleware;

namespace WebRtcMiddlewareXamarin
{
    public class MediaView : UIView, Webrtc.IRTCVideoViewDelegate
    {
        private bool _isCamera;
        private Webrtc.RTCEAGLVideoView _rendererView;
        private Webrtc.RTCCameraPreviewView _cameraView;
        private CGSize _rendererSize = CGSize.Empty;

        public MediaView()
        {
        }

        public void SetTrack(IMediaStreamTrack videoTrack)
        {
            var cameraDevices = Webrtc.RTCCameraVideoCapturer.CaptureDevices;
            _isCamera = cameraDevices.Any(device => device.ModelID == videoTrack.Id);

            var nativeVideoTrack = videoTrack.NativeObject as Webrtc.RTCVideoTrack;

            if (_isCamera)
            {
                _cameraView = new Webrtc.RTCCameraPreviewView();
                _cameraView.BackgroundColor = UIColor.Green; 
                AddSubview(_cameraView);

                var nativeVideoSource = nativeVideoTrack.Source;
                var videoCapturer = new Webrtc.RTCCameraVideoCapturer();
                videoCapturer.Delegate = nativeVideoSource;

                var cameraDevice = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                    ////                .FirstOrDefault(device => device.Position == cameraType.ToNative());
                    // Get the selected device by matching RTCMediaStreamTrack.TrackId with AVCaptureDevice.ModelID from
                    // RTCCameraVideoCapturer.CaptureDevices list.
                    .Single(device => device.ModelID == videoTrack.Id);

                var formats = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(cameraDevice);
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


                var format = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(cameraDevice)[6/*0*/];
                CMVideoFormatDescription videoFormatDescription = (CMVideoFormatDescription)format.FormatDescription;
                var capturerDimensions = videoFormatDescription.Dimensions;
                var capturerSize = new CGSize(capturerDimensions.Width, capturerDimensions.Height);
                var fps = 30;
                videoCapturer.StartCaptureWithDevice(cameraDevice, format, fps);

                _cameraView.CaptureSession = videoCapturer.CaptureSession;
            }
            else
            {
                _rendererView = new Webrtc.RTCEAGLVideoView();
                _rendererView.Delegate = this;
                AddSubview(_rendererView);

                nativeVideoTrack.AddRenderer(_rendererView);
            }

            SetNeedsLayout();
        }

        public override void LayoutSubviews()
        {
            System.Diagnostics.Debug.WriteLine($"@@@@@@ LayoutSubviews Bounds:{Bounds}");

            base.LayoutSubviews();

            CGRect frame = CGRect.Empty;
            if (_isCamera && _cameraView is not null)
            {
                // TODO: HOW TO GET CAMERA VIEW SIZE???
                // Currenty Portrait 3*4 aspect ratio is hard coded.
                if (Bounds.Width >= Bounds.Height)
                {
                    // View is landscape.
                    frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Width * (double)(4.0 / 3.0));
                }
                else
                {
                    // View is portrait.
                    frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Height * (double)(3.0 / 4.0), Bounds.Height);
                }
                _cameraView.Frame = frame;
                System.Diagnostics.Debug.WriteLine($"@@@@@@ _cameraView.Frame:{_cameraView.Frame}");
            }
            else if (!_isCamera && _rendererView is not null)
            {
                if (_rendererSize.Width > 0 && _rendererSize.Height > 0)
                {
                    nfloat scale = 0f;
                    frame = Bounds.WithAspectRatio(_rendererSize);
                    if (frame.Width >= frame.Height)
                        // Scale by height.
                        scale = Bounds.Height / frame.Height;
                    else
                        // Scale by width.
                        scale = Bounds.Width / frame.Width;
                    frame.Size = new CGSize(frame.Width * scale, frame.Height * scale);

                    //if (Bounds.Width >= Bounds.Height)
                    //{
                    //    // View is landscape.
                    //    if (_rendererSize.Width >= _rendererSize.Height)
                    //        // Renderer is landscape. Scale by height.
                    //        scale = Bounds.Height / _rendererSize.Height;
                    //    else
                    //        // Renderer is portrait. Scale by width.
                    //        scale = Bounds.Width / _rendererSize.Width;
                    //}
                    //else
                    //{
                    //    // View is portrait.
                    //    if (_rendererSize.Width >= _rendererSize.Height)
                    //        // Renderer is landscape. Scale by height.
                    //        scale = Bounds.Height / _rendererSize.Height;
                    //    else
                    //        // Renderer is portrait. Scale by width.
                    //        scale = Bounds.Width / _rendererSize.Width;
                    //}
                    //frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Width * scale, Bounds.Height * scale);
                    _rendererView.Frame = frame;
                    _rendererView.Center = new CGPoint(Bounds.GetMidX(), Bounds.GetMidY());
                }
                else
                    _rendererView.Frame = Bounds;
            }
        }

        [Export("videoView:didChangeVideoSize:")]
        public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        {
            if (videoView is Webrtc.RTCEAGLVideoView renderer && renderer.Superview is UIView parent)
            {
                System.Diagnostics.Debug.WriteLine($"@@@@@@ DidChangeVideoSize renderer.Frame:{renderer.Frame} " +
                    $"size:{size}");
                _rendererSize = size;
                SetNeedsLayout();
                //                parent.Frame = new CGRect(0, 0, size.Width, size.Height);
                //              parent.SetNeedsLayout();
            }
        }


    }
}

