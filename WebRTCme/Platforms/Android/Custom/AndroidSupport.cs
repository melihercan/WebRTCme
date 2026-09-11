using Android.Views;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using WebRTCme.Android;
using WebRTCme;
using Webrtc = Org.Webrtc;


namespace WebRTCme
{
    public static class AndroidSupport
    {
        public static Webrtc.VideoSource GetNativeVideoSource(IMediaStreamTrack videoTrack)
        {
            return ((MediaStreamTrack)videoTrack).GetNativeMediaSource() as Webrtc.VideoSource;
        }

        public static Webrtc.IEglBase GetNativeEglBase()
        {
            return WebRtc.NativeEglBase;
        }

        // The capturer already started for a camera track, keyed by track id, so binding the
        // same track to another renderer reuses it instead of opening the camera again.
        static readonly ConcurrentDictionary<string, Webrtc.ICameraVideoCapturer> _capturersByTrackId = new();

        // What getUserMedia asked this camera for, held until something binds the track and the
        // capture actually starts. Keyed by track id, which for a camera track is the camera id.
        static readonly ConcurrentDictionary<string, VideoConstraints> _formatsByTrackId = new();

        // 640x480 at 30fps, which is what this has always actually captured. The call used to read
        // StartCapture(480, 640, 30), but 480x640 is not a format any camera here publishes -
        // libwebrtc picked the nearest, which is 640x480, and the portrait pair only ever worked by
        // being closest to the landscape one it meant.
        const int DefaultCaptureWidth = 640;
        const int DefaultCaptureHeight = 480;
        const int DefaultCaptureFrameRate = 30;

        /// <summary>
        /// Records the size and frame rate a track's camera should be opened with.
        /// </summary>
        /// <remarks>
        /// The camera is not opened by <c>getUserMedia</c> - it is opened when a renderer first
        /// binds the track, which can be much later and has no access to the constraints. So they
        /// are left here on the way past.
        /// </remarks>
        public static void SetRequestedFormat(string trackId, VideoConstraints constraints)
        {
            if (trackId is not null && constraints is not null)
                _formatsByTrackId[trackId] = constraints;
        }

        /// <summary>
        /// Releases the camera captured for a track, if this is a track we started capture for.
        /// </summary>
        /// <remarks>
        /// Stopping the capturer is what actually turns the camera off; disabling the track only
        /// stops it being delivered. Remote tracks never had a capturer here, so this does
        /// nothing for them.
        /// </remarks>
        public static void StopCapture(string trackId)
        {
            if (trackId is null)
                return;

            _formatsByTrackId.TryRemove(trackId, out _);

            if (!_capturersByTrackId.TryRemove(trackId, out var videoCapturer))
                return;

            try
            {
                videoCapturer.StopCapture();
            }
            catch (Exception exception)
            {
                // Nothing useful to do about a camera that will not stop, and throwing out of
                // a teardown path helps nobody.
                Console.WriteLine($"Stopping the camera capture failed: {exception.Message}");
            }
        }

        public static void SetTrack(IMediaStreamTrack videoTrack, Webrtc.SurfaceViewRenderer rendererView,
            global::Android.Content.Context context/*, Webrtc.IEglBaseContext eglBaseContext*/)
        {
            // Nothing to render is not an error: a stream can carry audio and no video. Callers
            // are expected to check, and this is the second line of defence - the cast below
            // threw NullReferenceException from inside a MAUI property mapper, which surfaces as
            // an unhandled exception on the UI thread and takes the app down.
            if (videoTrack is null)
                return;

            var nativeVideoTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.VideoTrack;

            var cameraEnum = new Webrtc.Camera2Enumerator(context);
            var cameraDevices = cameraEnum.GetDeviceNames();
            var isCamera = cameraDevices.Any(device => device == videoTrack.Id);

            if (isCamera)
            {
                // Once per track, not once per view. This runs again every time the track is
                // bound to a renderer, and starting a second capture on a camera that is
                // already capturing closes the first session behind its own callback: that
                // session's onConfigured then calls setRepeatingRequest on a closed session
                // and takes the process down with "Session has been closed".
                _capturersByTrackId.GetOrAdd(videoTrack.Id, _ =>
                {
                    var nativeVideoSource = GetNativeVideoSource(videoTrack);
                    var videoCapturer = cameraEnum.CreateCapturer(videoTrack.Id, null);
                    videoCapturer.Initialize(
                        Webrtc.SurfaceTextureHelper.Create(
                            "CameraVideoCapturerThread",
                            GetNativeEglBase().EglBaseContext),
                        context,
                        nativeVideoSource.CapturerObserver);
                    var (width, height, frameRate) = FormatFor(videoTrack.Id);

                    // What the camera can actually do, beside what it was asked for. libwebrtc
                    // picks the closest supported format silently, so without this a request that
                    // came back as something else looks like the constraint was ignored.
                    System.Diagnostics.Debug.WriteLine(
                        $"######## Camera {videoTrack.Id} asked for {width}x{height}@{frameRate}; " +
                        $"supports {string.Join(", ", cameraEnum.GetSupportedFormats(videoTrack.Id)
                            .Select(format => $"{format.Width}x{format.Height}"))}");

                    videoCapturer.StartCapture(width, height, frameRate);
                    return videoCapturer;
                });
            }

            nativeVideoTrack.AddSink(rendererView);
        }

        /// <summary>
        /// The capture format for a track: what was asked for, or what this always used.
        /// </summary>
        /// <remarks>
        /// Width and height go through as written. <c>Camera2Enumerator</c> reports formats
        /// landscape - 1280x720, 640x480 - which is the same way round as a web constraint, so no
        /// swap is wanted. Swapping them is not a harmless mistake either: libwebrtc answers an
        /// unsupported request with the nearest supported format and says nothing, so asking a
        /// camera that publishes 1280x720 for 720x1280 opened it at 1088x1088, which is nearer to
        /// the transposed pair than the format actually meant.
        ///
        /// A constraint giving only one dimension is ignored rather than guessed at: half a size is
        /// not enough to open a camera with, and inventing the other half from an assumed aspect
        /// ratio would be a worse answer than the default.
        /// </remarks>
        static (int Width, int Height, int FrameRate) FormatFor(string trackId)
        {
            var constraints = trackId is not null &&
                _formatsByTrackId.TryGetValue(trackId, out var found)
                    ? found
                    : VideoConstraints.None;

            var frameRate = constraints.FrameRate ?? DefaultCaptureFrameRate;

            return constraints.HasSize
                ? (constraints.Width.Value, constraints.Height.Value, frameRate)
                : (DefaultCaptureWidth, DefaultCaptureHeight, frameRate);
        }
    }
}

