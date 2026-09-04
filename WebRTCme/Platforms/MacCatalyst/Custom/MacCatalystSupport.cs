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
using WebRTCme.MacCatalyst;

namespace WebRTCme
{
    public static class MacCatalystSupport
    {
        public static void SetCameraTrack(Webrtc.RTCCameraPreviewView _cameraView, IMediaStreamTrack videoTrack, 
            Webrtc.RTCCameraVideoCapturer _videoCapturer)
        {
            var nativeVideoTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            var nativeVideoSource = nativeVideoTrack.Source;
            _videoCapturer.Delegate = (Webrtc.IRTCVideoCapturerDelegate)nativeVideoSource;

            var cameraDevice = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                ////                .FirstOrDefault(device => device.Position == cameraType.ToNative());
                // The track id is the device's UniqueID (see MediaStream.Create), so match on
                // that - ModelID is not unique and does not identify the chosen device.
                .Single(device => device.UniqueID == videoTrack.Id);

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

        public static void SetRendererTrack(Webrtc.RTCMTLVideoView/****RTCEAGLVideoView****/ rendererView, IMediaStreamTrack videoTrack)
        {
            var nativeVideoTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            nativeVideoTrack.AddRenderer((Webrtc.IRTCVideoRenderer)rendererView);
        }

    }
}

