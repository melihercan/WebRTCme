using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using HomeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using WebRTCme.iOS;

namespace WebRTCme
{
    public static class IosSupport
    {
        public static void SetCameraTrack(Webrtc.RTCCameraPreviewView _cameraView, IMediaStreamTrack videoTrack, 
            Webrtc.RTCCameraVideoCapturer _videoCapturer)
        {
            var nativeVideoTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            var nativeVideoSource = nativeVideoTrack.Source;
            _videoCapturer.Delegate = nativeVideoSource;

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
            _videoCapturer.StartCaptureWithDevice(cameraDevice, format, fps);

            _cameraView.CaptureSession = _videoCapturer.CaptureSession;

        }

        public static void SetRendererTrack(Webrtc.RTCEAGLVideoView rendererView, IMediaStreamTrack videoTrack)
        {
            var nativeVideoTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            nativeVideoTrack.AddRenderer(rendererView);
        }

    }
}

