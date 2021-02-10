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
using System.Threading.Tasks;

namespace WebRtc.iOS
{
    internal class ApiExtensions : ApiBase, IApiExtensions
    {
        public static IApiExtensions Create() => new ApiExtensions();

        private ApiExtensions() { }

        public IVideoCapturer SetCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType, 
            MediaStreamConstraints mediaStreamConstraints)
        {
            if (cameraType == CameraType.Default)
                cameraType = CameraType.Front;

            var nativeTrack = cameraVideoTrack.NativeObject as Webrtc.RTCVideoTrack;
            var videoSource = nativeTrack.Source;

            var videoCapturer = new Webrtc.RTCCameraVideoCapturer();
            videoCapturer.Delegate = videoSource;

            var cameraDevice = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                ////                .FirstOrDefault(device => device.Position == cameraType.ToNative());
                // Get the selected device by matching RTCMediaStreamTrack.TrackId with AVCaptureDevice.ModelID from
                // RTCCameraVideoCapturer.CaptureDevices list.
                .Single(device => device.ModelID == cameraVideoTrack.Id);

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

            return VideoCapturer.Create(videoCapturer);
        }

        public IEglBaseContext GetEglBaseContext() =>
            throw new NotSupportedException("EGL base context is not supported");





        //public void SetCameraCapturer(IMediaStreamTrack videoTrack)
        //{
        //    var nativeVideoTrack = videoTrack.NativeObject as Webrtc.RTCVideoTrack;
        //    var nativeVideoCapturer = new Webrtc.RTCCameraVideoCapturer();
        //    nativeVideoCapturer.Delegate = nativeVideoTrack.Source;

        //    // Get the selected device by matching RTCMediaStreamTrack.TrackId with AVCaptureDevice.ModelId from
        //    // RTCCameraVideoCapturer.CaptureDevices list.
        //    var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
        //        .First(capturer => capturer.ModelID == videoTrack.Id);


        //    int width = 640;
        //    int height = Convert.ToInt32(640 * 16 / 9f);
        //    int fps = 30;
        //    var formats = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device);

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

        //    nativeVideoCapturer.StartCaptureWithDevice(device, targetFormat, fps);

        //}

        //public (IVideoView, IVideoView) GetCameraViewAndRenderer()
        //{
        //    var rendererView = new Webrtc.RTCEAGLVideoView();
        //    rendererView.Delegate = this;
        //    var videoView = new UIView();
        //    videoView.AddSubview(rendererView);
        //    return (View.Create(videoView), View.Create(rendererView));


        //}

        //public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        //{
        //    System.Diagnostics.Debug.WriteLine($"{nameof(DidChangeVideoSize)}");

        //    //if (videoView is Webrtc.RTCEAGLVideoView rendererView &&
        //    //    rendererView.Superview is UIView parentView)
        //    //{
        //    //    var constraints = parentView.Constraints
        //    //        .Where(lc => lc.SecondAttribute == NSLayoutAttribute.Width ||
        //    //                     lc.SecondAttribute == NSLayoutAttribute.Height)
        //    //        .ToArray();
        //    //    parentView.RemoveConstraints(constraints);

        //    //    var isLandscape = size.Width > size.Height;

        //    //    if (isLandscape)
        //    //    {
        //    //        parentView.AddConstraints(new[]
        //    //        {
        //    //            rendererView.WithSameWidth(parentView),
        //    //            rendererView.Height()
        //    //                        .EqualTo()
        //    //                        .WidthOf(parentView)
        //    //                        .WithMultiplier(size.Height / size.Width)
        //    //        });
        //    //    }
        //    //    else
        //    //    {
        //    //        parentView.AddConstraints(new[]
        //    //        {
        //    //            rendererView.Width()
        //    //                        .EqualTo()
        //    //                        .HeightOf(parentView)
        //    //                        .WithMultiplier(size.Width / size.Height),
        //    //            rendererView.WithSameHeight(parentView)
        //    //        });
        //    //    }
        //    //}

        //}

    }
}

