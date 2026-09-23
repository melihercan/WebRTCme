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
        /// <summary>
        /// The capturer feeding each camera track, keyed by the native track.
        /// </summary>
        /// <remarks>
        /// <para>Keyed by the native object rather than by the track's id, and that is the fix for
        /// two faults found on 2026-09-23. A camera track's id is its device's <c>UniqueID</c>, so
        /// every track ever opened on one camera has the same id. Keyed by it, the first call's
        /// capturer was found and reused by every later call, still feeding the first call's video
        /// source: from the second call on, <b>the peer received no video at all</b> - measured as
        /// <c>video:0</c> inbound on Windows while the Mac's own preview showed the camera. And
        /// since nothing ever stopped it, <b>the camera stayed on after the call</b> for as long
        /// as the app ran.</para>
        /// </remarks>
        static readonly ConcurrentDictionary<IntPtr, Webrtc.RTCCameraVideoCapturer> _capturersByTrack = new();

        /// <summary>
        /// Opens the camera into a new camera track, so the track is live when it is handed back -
        /// what a browser's <c>getUserMedia</c> does.
        /// </summary>
        /// <remarks>
        /// <para>Capture used to start only when a view first bound the track, which meant a track
        /// nobody displayed sent no video, and the capture session was started with a preview layer
        /// being attached to it moments later. Attaching a preview layer to a session makes
        /// AVFoundation reconfigure it and reopen the device, and on a 2018 Mac mini that cost
        /// <b>9 to 19 seconds with no frames</b> - measured frame by frame on 2026-09-23. It was the
        /// "LED flashes and the picture takes ten seconds" seen on every join. Nothing attaches a
        /// preview layer now: the local tile renders the track, as a remote one does.</para>
        /// <para>The frames go through <see cref="CameraCapture.Upright"/> on the way to the
        /// track's source. <c>RTCCameraVideoCapturer</c> tags them from <c>UIDevice.orientation</c>,
        /// which on a Mac describes nothing, so without that they arrive at a peer a quarter turn
        /// out.</para>
        /// </remarks>
        internal static void StartCamera(MediaStreamTrack track)
        {
            var nativeVideoTrack = (Webrtc.RTCVideoTrack)track.NativeObject;
            var capturer = new Webrtc.RTCCameraVideoCapturer
            {
                Delegate = CameraCapture.Upright(
                    (Webrtc.IRTCVideoCapturerDelegate)nativeVideoTrack.Source)
            };

            // The track id is the device's UniqueID (see MediaStream.Create); ModelID is not unique.
            var cameraDevice = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .Single(device => device.UniqueID == track.Id);

            var (format, fps) = SelectFormat(cameraDevice, track.Id);
            Console.WriteLine($"######## camera {cameraDevice.LocalizedName} opening");
            capturer.StartCaptureWithDevice(cameraDevice, format, fps);

            _capturersByTrack[nativeVideoTrack.Handle] = capturer;
        }

        /// <summary>Stops the camera feeding a track, if one is. Called when the track is stopped.</summary>
        internal static void StopCamera(MediaStreamTrack track)
        {
            if (track.NativeObject is Webrtc.RTCVideoTrack nativeVideoTrack
                && _capturersByTrack.TryRemove(nativeVideoTrack.Handle, out var capturer))
            {
                Console.WriteLine($"######## camera {track.Id} closed");
                capturer.StopCapture();
            }
        }

        /// <summary>
        /// Shows a camera track's capture session in a preview view.
        /// </summary>
        /// <remarks>
        /// Kept for callers that want AVFoundation's own preview layer, and no longer used by
        /// WebRTCme's media view - which renders the track instead. Attaching a preview layer to a
        /// running session makes AVFoundation reconfigure it, and on some Macs the camera then
        /// delivers nothing for ten seconds or more. This starts nothing: the track's own capturer,
        /// started when the track was opened, is the only one.
        /// </remarks>
        public static void SetCameraTrack(Webrtc.RTCCameraPreviewView cameraView, IMediaStreamTrack videoTrack)
        {
            if (((MediaStreamTrack)videoTrack).NativeObject is Webrtc.RTCVideoTrack nativeVideoTrack
                && _capturersByTrack.TryGetValue(nativeVideoTrack.Handle, out var capturer))
                cameraView.CaptureSession = capturer.CaptureSession;
        }

        /// <summary>Takes a renderer off the track it was drawing, before the view shows another.</summary>
        public static void RemoveRendererTrack(Webrtc.RTCMTLVideoView rendererView, IMediaStreamTrack videoTrack)
        {
            if (videoTrack is null)
                return;
            if (((MediaStreamTrack)videoTrack).NativeObject is Webrtc.RTCVideoTrack nativeVideoTrack)
                nativeVideoTrack.RemoveRenderer((Webrtc.IRTCVideoRenderer)rendererView);
        }

        /// <summary>Starts a caller-supplied capturer on a track's camera and shows it.</summary>
        [Obsolete("A camera track now owns its capturer, started when the track is opened. This " +
                  "starts a second one on the same device; use SetCameraTrack(view, track).")]
        public static void SetCameraTrack(Webrtc.RTCCameraPreviewView _cameraView, IMediaStreamTrack videoTrack,
            Webrtc.RTCCameraVideoCapturer _videoCapturer)
        {
            var nativeVideoTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            _videoCapturer.Delegate = CameraCapture.Upright(
                (Webrtc.IRTCVideoCapturerDelegate)nativeVideoTrack.Source);
            var cameraDevice = Webrtc.RTCCameraVideoCapturer.CaptureDevices
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
        /// What a camera track was opened with, for a caller asking a track about itself.
        /// </summary>
        /// <remarks>
        /// The requested format, not a measured one, and not the format
        /// <see cref="SelectFormat"/> settles on either - that choice is not made until a view
        /// binds the track and capture starts, which can be long after anything asks this.
        ///
        /// Null for a track that is not a camera this opened, which the caller reports as "no
        /// settings" rather than as zeroes.
        /// </remarks>
        public static (int Width, int Height, int FrameRate)? RequestedFormatFor(string trackId)
        {
            if (trackId is null || !_formatsByTrackId.TryGetValue(trackId, out var constraints))
                return null;

            return (
                constraints.HasSize ? constraints.Width.Value : DefaultCaptureWidth,
                constraints.HasSize ? constraints.Height.Value : DefaultCaptureHeight,
                constraints.FrameRate ?? DefaultCaptureFrameRate);
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

