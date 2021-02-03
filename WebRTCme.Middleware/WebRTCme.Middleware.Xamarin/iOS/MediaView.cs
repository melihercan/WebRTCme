using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Cirrious.FluentLayouts;
using Cirrious.FluentLayouts.Touch;
using WebRTCme;
using CoreMedia;
using WebRTCme.Middleware;

namespace WebRtcMiddlewareXamarin
{

    public class MediaView : UIView
    {
        public UIView LocalVideoView { get; }

        public MediaView(UIView view)
        {
            LocalVideoView = view;
            AddSubview(LocalVideoView);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            LocalVideoView.Frame = Bounds;
        }
    }



#if false

    public class MediaView : UIView , Webrtc.IRTCVideoViewDelegate
    {

        //public UIView LocalVideoView { get; }

        //public MediaView(UIView view)
        //{
        //    LocalVideoView = view;
        //    AddSubview(LocalVideoView);
        //}

        private readonly IMediaStreamTrack _track;
        private readonly IVideoView _view;
        private readonly IVideoRenderer _renderer;
        private readonly IVideoCapturer _capturer;

        private readonly Webrtc.RTCVideoTrack _nativeTrack;
        private readonly UIView _nativeView;
        private readonly Webrtc.RTCEAGLVideoView _nativeRenderer;
        private readonly Webrtc.RTCCameraVideoCapturer _nativeCapturer;



        UIView _localView;
        Webrtc.RTCCameraVideoCapturer _videoCapturer;
        Webrtc.RTCVideoTrack _localVideoTrack;
        Webrtc.RTCEAGLVideoView _localRenderView;

        CGSize _capturerSize = CGSize.Empty;
        CGSize _rendererSize = CGSize.Empty;

        private Webrtc.RTCPeerConnectionFactory _peerConnectionFactory;

        public MediaView(IMediaStreamTrack track, IVideoView view, IVideoRenderer renderer, IVideoCapturer capturer)
        {

            //// TESTING by using RTCEAGLVideoView

            /*_localView.*/
            BackgroundColor = UIColor.SystemBlueColor;

#if true
            var videoEncoderFactory = new Webrtc.RTCDefaultVideoEncoderFactory();
            var videoDecoderFactory = new Webrtc.RTCDefaultVideoDecoderFactory();
            _peerConnectionFactory = new Webrtc.RTCPeerConnectionFactory(videoEncoderFactory, videoDecoderFactory);

            _localRenderView = new Webrtc.RTCEAGLVideoView();
            _localRenderView.Delegate = this;
            _localView = new UIView();
            _localView.BackgroundColor = UIColor.SystemBlueColor;
            _localView.AddSubview(_localRenderView);
            AddSubview(_localView);


            var videoSource = _peerConnectionFactory.VideoSource;

            _videoCapturer = new Webrtc.RTCCameraVideoCapturer();
            _videoCapturer.Delegate = videoSource;
            
            _localVideoTrack = _peerConnectionFactory.VideoTrackWithSource(videoSource, "video0");


            var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Front);

            ////////////////// TODO: MAKE CAPTURE PORTRAIT


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
            var fps = 30;// 60;// 30;// GetFpsByFormat(format);
            _videoCapturer.StartCaptureWithDevice(device, format, fps);


            //_localRenderView = new Webrtc.RTCEAGLVideoView();
            //_localRenderView.SetSize(new CGSize(_capturerSize.Width, _capturerSize.Height));
            //_localRenderView.Delegate = this;
            //_localView = new UIView();
            //_localView.BackgroundColor = UIColor.SystemBlueColor;
            //_localView.AddSubview(_localRenderView);
            //AddSubview(_localView);


            //_videoCapturer.StartCaptureWithDevice(device, format, fps);
            _localVideoTrack.AddRenderer(_localRenderView);

#endif




#if false
            //// WORKING by using RTCCameraPreviewView
            var nativeCameraVideoCapturer = new Webrtc.RTCCameraVideoCapturer();

            //// TODO USE constraints to set the below values
            //// Get the selected device by matching RTCMediaStreamTrack.TrackId with AVCaptureDevice.ModelId from
            //// RTCCameraVideoCapturer.CaptureDevices list.
            var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .First(capturer => capturer.ModelID == track.Id);
            var format = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
            var fps = GetFpsByFormat(format);
            nativeCameraVideoCapturer.StartCaptureWithDevice(device, format, fps);

            var nativeCameraPreviewView = new Webrtc.RTCCameraPreviewView();
            nativeCameraPreviewView.CaptureSession = nativeCameraVideoCapturer.CaptureSession;
            _localView = nativeCameraPreviewView;

            AddSubview(_localView);

             int GetFpsByFormat(AVCaptureDeviceFormat fmt)
             {
                 const float _frameRateLimit = 30.0f;

                 var maxSupportedFps = 0d;
                 foreach (var fpsRange in fmt.VideoSupportedFrameRateRanges)
                     maxSupportedFps = Math.Max(maxSupportedFps, fpsRange.MaxFrameRate);

                 return (int)Math.Min(maxSupportedFps, _frameRateLimit);
             }
#endif


            //Webrtc.RTCCameraVideoCapturer _videoCapturer;
            //Webrtc.RTCVideoTrack _localVideoTrack;
            //Webrtc.RTCEAGLVideoView _localRenderView;


            //_localRenderView = new Webrtc.RTCEAGLVideoView();
            //_localRenderView.Delegate = this;
            //_localView = new UIView();
            //_localView.AddSubview(_localRenderView);

            //AddSubview(_localView);

            //_localVideoTrack = track.NativeObject as Webrtc.RTCVideoTrack;

            //_videoCapturer = new Webrtc.RTCCameraVideoCapturer();
            //_videoCapturer.Delegate = _localVideoTrack.Source;

            //var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
            //    .First(capturer => capturer.ModelID == track.Id);
            //var format = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
            //var fps = 30;// GetFpsByFormat(format);
            //_videoCapturer.StartCaptureWithDevice(device, format, fps);

            //_localVideoTrack.AddRenderer(_localRenderView);





            //_track = track;
            //_view = view;
            //_renderer = renderer;
            //_capturer = capturer;

            //_nativeTrack = track.NativeObject as Webrtc.RTCVideoTrack;
            //_nativeView = view.NativeObject as UIView;
            //_nativeRenderer = renderer.NativeObject as Webrtc.RTCEAGLVideoView;
            //_nativeCapturer = capturer.NativeObject as Webrtc.RTCCameraVideoCapturer;

            //_nativeRenderer.Delegate = this;
            //_nativeView.AddSubview(_nativeRenderer);

            //AddSubview(_nativeView);

            //_nativeCapturer.Delegate = _nativeTrack.Source;


            //int width = 640;
            //int height = Convert.ToInt32(640 * 16 / 9f);
            //int fps = 30;

            //var devices = Webrtc.RTCCameraVideoCapturer.CaptureDevices;
            //var targetDevice = devices.FirstOrDefault(device => device.ModelID == _track.Id);


            //if (targetDevice != null)
            //{
            //    var formats = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(targetDevice);

            //    var targetFormat = formats.FirstOrDefault(f =>
            //    {
            //        var description = f.FormatDescription;
            //        if (description is CMVideoFormatDescription videoDescription)
            //        {
            //            var dimensions = videoDescription.Dimensions;
            //            if ((dimensions.Width == width && dimensions.Height == height) ||
            //                (dimensions.Width == width))
            //            {
            //                return true;
            //            }
            //        }

            //        return false;
            //    });

            //    if (targetFormat != null)
            //    {
            //        _nativeCapturer.StartCaptureWithDevice(targetDevice, targetFormat, fps);
            //    }

            //}



            //_nativeTrack.AddRenderer(_nativeRenderer);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            System.Diagnostics.Debug.WriteLine($"@@@@@@ LayoutSubviews(Begin)  Bounds:{Bounds} _capturerSize:{_capturerSize} _rendererSize:{_rendererSize}");

            if (_rendererSize.Width > 0 && _rendererSize.Height > 0)
            {
                var frame = new CGRect(0, 0, _rendererSize.Width, _rendererSize.Height);
                _localView.Frame = frame; 
            }
            else
            {
                //_localRenderView.SetSize(new CGSize(_capturerSize.Width, _capturerSize.Height));
                _localView.Frame = Bounds;
            }

            System.Diagnostics.Debug.WriteLine($"@@@@@@ LayoutSubviews(End)  _localView:{_localView.Frame} _localRenderView:{_localRenderView.Frame}");

        }

        [Export("videoView:didChangeVideoSize:")]
        public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        {
            System.Diagnostics.Debug.WriteLine($"@@@@@@ DidChangeVideoSize videoView.Frame:{((Webrtc.RTCEAGLVideoView)videoView).Frame} size:{size}");

            _rendererSize = size;
            SetNeedsLayout();


            //videoView.SetSize(size);


            //if (videoView is Webrtc.RTCEAGLVideoView rendererView &&
            //    rendererView.Superview is UIView parentView)
            //{
            //    var constraints = parentView.Constraints
            //        .Where(lc => lc.SecondAttribute == NSLayoutAttribute.Width ||
            //                     lc.SecondAttribute == NSLayoutAttribute.Height)
            //        .ToArray();
            //    parentView.RemoveConstraints(constraints);

            //    var isLandscape = size.Width > size.Height;

            //    if (isLandscape)
            //    {
            //        parentView.AddConstraints(new[]
            //        {
            //                rendererView.WithSameWidth(parentView),
            //                rendererView.Height()
            //                    .EqualTo()
            //                    .WidthOf(parentView)
            //                    .WithMultiplier(size.Height / size.Width)
            //            });
            //    }
            //    else
            //    {
            //        parentView.AddConstraints(new[]
            //        {
            //                rendererView.Width()
            //                    .EqualTo()
            //                    .HeightOf(parentView)
            //                    .WithMultiplier(size.Width / size.Height),
            //                rendererView.WithSameHeight(parentView)
            //            });
            //    }
            //}
        }


        //internal class RemoteView : Webrtc.RTCEAGLVideoView, Webrtc.IRTCVideoViewDelegate
        //{
        //    public RemoteView()
        //    {
        //        Delegate = this;
        //    }

        //    [Export("videoView:didChangeVideoSize:")]
        //    public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        //    {
        //    }

        //}
    }
#endif
}

