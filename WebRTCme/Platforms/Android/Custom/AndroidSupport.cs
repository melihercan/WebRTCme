using Android.Views;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Android;
using WebRTCme.Platforms.Android.Custom;
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

        // The screen capturer, when one is running. At most one: Android grants a projection for a
        // single capture session, so a second share means a second permission dialog and the first
        // session ending anyway.
        static Webrtc.ScreenCapturerAndroid _screenCapturer;
        static string _screenTrackId;

        #region Getting the camera back

        // The tracks a recovery is already running for. A lost camera reports itself more than
        // once - a disconnect is followed by an error from every failed reopen - and each of those
        // must feed the loop that is already running rather than start another one beside it.
        static readonly ConcurrentDictionary<string, byte> _recoveringTrackIds = new();

        // How long to keep trying. Face unlock holds the camera for a couple of seconds, the
        // system camera app for as long as the user is in it; past a minute of failing there is
        // something wrong that retrying will not fix, and a loop nobody can see is worse than a
        // still picture somebody can.
        const int RecoveryAttempts = 15;
        const int RecoveryFirstDelayMs = 500;
        const int RecoveryMaxDelayMs = 5000;

        /// <summary>
        /// Reopens the camera for a track whose capture the system took away.
        /// </summary>
        /// <remarks>
        /// Only for a track that is still a live local capture: a camera lost because the call
        /// ended is not one to fight over, and <see cref="LocalCaptureStopped"/> is what says
        /// which is which.
        ///
        /// Each attempt stops the capturer before starting it. The capturer's state after a
        /// disconnect is a closed session it has not let go of, and starting on top of one of
        /// those is what takes the process down with "Session has been closed".
        ///
        /// Success is a frame, not a returned call: <c>startCapture</c> posts the work to the
        /// capturer's own thread and comes back immediately, so a start that fails because the
        /// other client still holds the camera looks exactly like one that worked.
        /// <see cref="CameraRecovered"/> is what actually ends the loop.
        /// </remarks>
        public static void CameraLost(string trackId, string reason)
        {
            if (trackId is null)
                return;

            CameraEcho($"camera {trackId} lost: {reason}");

            if (!_localCaptureTrackIds.ContainsKey(trackId))
                return;

            if (!_recoveringTrackIds.TryAdd(trackId, 0))
                return;

            _ = Task.Run(() => RecoverCameraAsync(trackId));
        }

        /// <summary>
        /// Ends a recovery, because a frame arrived.
        /// </summary>
        public static void CameraRecovered(string trackId)
        {
            if (trackId is not null && _recoveringTrackIds.TryRemove(trackId, out _))
                CameraEcho($"camera {trackId} is capturing again");
        }

        static async Task RecoverCameraAsync(string trackId)
        {
            var delay = RecoveryFirstDelayMs;

            for (var attempt = 1; attempt <= RecoveryAttempts; attempt++)
            {
                await Task.Delay(delay).ConfigureAwait(false);
                delay = Math.Min(delay * 2, RecoveryMaxDelayMs);

                // These say the loop is over from opposite directions: the track has stopped, or
                // a frame has arrived and CameraRecovered has already cleared us.
                if (!_localCaptureTrackIds.ContainsKey(trackId) ||
                    !_recoveringTrackIds.ContainsKey(trackId))
                    return;

                if (!_capturersByTrackId.TryGetValue(trackId, out var videoCapturer))
                    return;

                try
                {
                    videoCapturer.StopCapture();
                }
                catch (Exception exception)
                {
                    // Expected: there is usually no session left to stop. It still has to be
                    // asked, because when there is one, starting over it is fatal.
                    CameraEcho($"camera {trackId} would not stop before attempt {attempt}: " +
                        $"{exception.Message}");
                }

                try
                {
                    var (width, height, frameRate) = FormatFor(trackId);
                    videoCapturer.StartCapture(width, height, frameRate);
                    CameraEcho($"camera {trackId} reopen attempt {attempt} of {RecoveryAttempts}");
                }
                catch (Exception exception)
                {
                    CameraEcho($"camera {trackId} reopen attempt {attempt} threw: {exception.Message}");
                }
            }

            if (_recoveringTrackIds.TryRemove(trackId, out _))
                CameraEcho($"camera {trackId} did not come back after {RecoveryAttempts} attempts");
        }

        internal static void CameraEcho(string line)
        {
            Console.WriteLine($"######## {line}");
            System.Diagnostics.Debug.WriteLine($"######## {line}");
        }

        #endregion

        #region Keeping the call alive

        // The local capture tracks that currently exist. While there is at least one, the app
        // needs to be a foreground service or Android will take the camera away and freeze the
        // process - see CallForegroundService for what that does to a call.
        static readonly ConcurrentDictionary<string, byte> _localCaptureTrackIds = new();

        /// <summary>
        /// Notes that a local camera or microphone track now exists, starting the call service.
        /// </summary>
        /// <remarks>
        /// Driven by capture rather than by the app, because the app does not reliably know: a call
        /// is over when the tracks stop, and tracks stop from teardown paths as well as from a
        /// hang-up button.
        /// </remarks>
        public static void LocalCaptureStarted(string trackId)
        {
            if (trackId is null)
                return;

            var wasIdle = _localCaptureTrackIds.IsEmpty;
            _localCaptureTrackIds[trackId] = 0;

            if (wasIdle)
                CallForegroundService.Start(global::Android.App.Application.Context);
        }

        /// <summary>
        /// Notes that a local track has stopped, ending the call service once none are left.
        /// </summary>
        public static void LocalCaptureStopped(string trackId)
        {
            if (trackId is null || !_localCaptureTrackIds.TryRemove(trackId, out _))
                return;

            if (_localCaptureTrackIds.IsEmpty)
                CallForegroundService.Stop(global::Android.App.Application.Context);
        }

        #endregion

        /// <summary>
        /// Starts capturing the screen into a video source, once permission has been granted.
        /// </summary>
        /// <remarks>
        /// Unlike the camera, this starts at track creation rather than when a renderer binds the
        /// track. There is no device to look up later - the capturer is built around the one-shot
        /// Intent the permission dialog returned - and a screen share that only began once somebody
        /// displayed it locally would be a surprising thing to ship.
        ///
        /// Size comes from the display rather than from constraints. Capturing a screen at
        /// something other than its own aspect ratio produces letterboxing baked into the frames,
        /// which is worse than sending the real thing and letting the far side fit it.
        /// </remarks>
        public static void StartScreenCapture(
            IMediaStreamTrack videoTrack,
            global::Android.Content.Intent permission,
            int width,
            int height,
            int frameRate)
        {
            StopScreenCapture();

            var nativeVideoSource = GetNativeVideoSource(videoTrack);

            var capturer = new Webrtc.ScreenCapturerAndroid(
                permission, new ProjectionStoppedCallback(videoTrack));
            capturer.Initialize(
                Webrtc.SurfaceTextureHelper.Create("ScreenCapturerThread", GetNativeEglBase().EglBaseContext),
                global::Android.App.Application.Context,
                nativeVideoSource.CapturerObserver);
            capturer.StartCapture(width, height, frameRate);

            _screenCapturer = capturer;
            _screenTrackId = videoTrack.Id;
        }

        /// <summary>
        /// Stops capturing the screen and takes the foreground notification down with it.
        /// </summary>
        public static void StopScreenCapture()
        {
            var capturer = _screenCapturer;
            _screenCapturer = null;
            _screenTrackId = null;

            if (capturer is null)
                return;

            try
            {
                capturer.StopCapture();
                capturer.Dispose();
            }
            catch (Exception exception)
            {
                // Same reasoning as the camera: nothing useful to do about a capturer that will
                // not stop, and throwing out of a teardown path helps nobody.
                Console.WriteLine($"Stopping the screen capture failed: {exception.Message}");
            }
            finally
            {
                ScreenCaptureService.Stop(global::Android.App.Application.Context);
            }
        }

        /// <summary>Whether this track is the screen rather than a camera.</summary>
        public static bool IsScreenTrack(string trackId) =>
            trackId is not null && trackId == _screenTrackId;

        /// <summary>
        /// Notices when the user stops sharing from the system UI rather than from the app.
        /// </summary>
        /// <remarks>
        /// Android puts its own "stop sharing" control in the status bar, so the projection can end
        /// without this app being asked. Without this the capturer would keep running against a
        /// dead projection and the notification would stay up.
        /// </remarks>
        class ProjectionStoppedCallback : global::Android.Media.Projection.MediaProjection.Callback
        {
            readonly IMediaStreamTrack _track;

            public ProjectionStoppedCallback(IMediaStreamTrack track) => _track = track;

            public override void OnStop()
            {
                base.OnStop();
                Console.WriteLine("######## The user stopped screen sharing from the system UI.");

                // Capture first, then the track. StopScreenCapture clears the ids it keys on, so
                // the Stop below does not come back round - and ending the track is what lets
                // anything above here notice. Without it the far side keeps a tile showing the
                // last frame, indefinitely, with nothing to say the share is over.
                StopScreenCapture();

                try { _track?.Stop(); }
                catch (Exception exception)
                {
                    Console.WriteLine($"Ending the screen track failed: {exception.Message}");
                }
            }
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
                    // The events handler was null here, which is why a camera the system took
                    // away never came back - see CameraEvents.
                    var videoCapturer = cameraEnum.CreateCapturer(
                        videoTrack.Id, new CameraEvents(videoTrack.Id));
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
        /// What a camera track was opened with, for a caller asking a track about itself.
        /// </summary>
        /// <remarks>
        /// The requested format rather than a measured one. Android's <c>VideoTrack</c> does not
        /// report the size it is delivering, and libwebrtc picks the nearest supported format to
        /// what it was asked for without saying which - so this can differ from reality by
        /// whatever that substitution did. It is still a far better answer than throwing, which is
        /// what <c>GetSettings</c> did before: the simulcast ladder is chosen from the track's
        /// height, so on Android it always fell to the catch and took the fallback ladder.
        ///
        /// Returns null for a track that is not a camera - a remote track, or an audio one - which
        /// the caller reports as "no settings" rather than as zeroes.
        /// </remarks>
        public static (int Width, int Height, int FrameRate)? RequestedFormatFor(string trackId)
        {
            if (trackId is null || !_formatsByTrackId.ContainsKey(trackId))
                return null;

            return FormatFor(trackId);
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

