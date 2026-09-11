using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using HomeKit;
using System;
using System.Collections.Concurrent;
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

            var (format, fps) = SelectFormat(cameraDevice, videoTrack.Id);
            _videoCapturer.StartCaptureWithDevice(cameraDevice, format, fps);

            _cameraView.CaptureSession = _videoCapturer.CaptureSession;

        }

        // What getUserMedia asked a camera for, held until something binds the track and capture
        // actually starts. Keyed by track id, which for a camera track is the device's UniqueID.
        static readonly ConcurrentDictionary<string, VideoConstraints> _formatsByTrackId = new();

        // 640x480 at 30fps when nothing is asked for, matching Android's default so the two agree.
        const int DefaultCaptureWidth = 640;
        const int DefaultCaptureHeight = 480;
        const int DefaultCaptureFrameRate = 30;

        /// <summary>
        /// Records the size and frame rate a track's camera should be opened with.
        /// </summary>
        public static void SetRequestedFormat(string trackId, VideoConstraints constraints)
        {
            if (trackId is not null && constraints is not null)
                _formatsByTrackId[trackId] = constraints;
        }

        /// <summary>
        /// The supported capture format closest to what was asked for, and a frame rate it allows.
        /// </summary>
        /// <remarks>
        /// This used to be <c>SupportedFormatsForDevice(device)[6]</c>. The index is meaningless -
        /// the list differs per device and per iOS version, so index 6 is a different resolution on
        /// every phone and is out of range on a camera that publishes fewer than seven formats.
        ///
        /// Closest wins by summed dimension difference rather than by exact match, because a camera
        /// rarely publishes the size a caller asks for and refusing to start is a worse answer than
        /// starting near it. Ties break towards the smaller format: overshooting the request costs
        /// bandwidth and battery to produce detail the caller said it did not want.
        ///
        /// The frame rate is clamped to what the chosen format actually supports. Asking for more
        /// than the maximum is rejected outright by AVFoundation.
        /// </remarks>
        static (AVCaptureDeviceFormat Format, int FrameRate) SelectFormat(
            AVCaptureDevice cameraDevice, string trackId)
        {
            var constraints = trackId is not null &&
                _formatsByTrackId.TryGetValue(trackId, out var found)
                    ? found
                    : VideoConstraints.None;

            var wantedWidth = constraints.HasSize ? constraints.Width.Value : DefaultCaptureWidth;
            var wantedHeight = constraints.HasSize ? constraints.Height.Value : DefaultCaptureHeight;

            var formats = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(cameraDevice);
            if (formats.Length == 0)
                throw new InvalidOperationException(
                    $"Camera '{cameraDevice.UniqueID}' publishes no capture formats.");

            var format = formats
                .OrderBy(candidate => Distance(candidate, wantedWidth, wantedHeight))
                .ThenBy(Pixels)
                .First();

            var maxFrameRate = format.VideoSupportedFrameRateRanges
                .Aggregate(0d, (highest, range) => Math.Max(highest, range.MaxFrameRate));

            var frameRate = constraints.FrameRate ?? DefaultCaptureFrameRate;
            if (maxFrameRate > 0 && frameRate > maxFrameRate)
                frameRate = (int)maxFrameRate;

            return (format, frameRate);

            static int Distance(AVCaptureDeviceFormat format, int width, int height)
            {
                var dimensions = ((CMVideoFormatDescription)format.FormatDescription).Dimensions;
                return Math.Abs(dimensions.Width - width) + Math.Abs(dimensions.Height - height);
            }

            static int Pixels(AVCaptureDeviceFormat format)
            {
                var dimensions = ((CMVideoFormatDescription)format.FormatDescription).Dimensions;
                return dimensions.Width * dimensions.Height;
            }
        }

        public static void SetRendererTrack(Webrtc.RTCMTLVideoView/****RTCEAGLVideoView****/ rendererView, IMediaStreamTrack videoTrack)
        {
            var nativeVideoTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            nativeVideoTrack.AddRenderer((Webrtc.IRTCVideoRenderer)rendererView);
        }

    }
}

