using System;
using System.Linq;
using System.Threading.Tasks;
using CoreMedia;
using CoreVideo;
using Foundation;
using ScreenCaptureKit;
using WebRTCme.MacCatalyst;

namespace WebRTCme
{
    /// <summary>
    /// Screen capture for <c>getDisplayMedia</c>, on ScreenCaptureKit.
    /// </summary>
    /// <remarks>
    /// <para>ScreenCaptureKit rather than ReplayKit, which is what the iOS side of this library
    /// uses. Both are available to a Mac Catalyst app and they capture different things:
    /// ReplayKit's in-process recorder gives you the application's own content, and
    /// ScreenCaptureKit gives you the display. On a Mac the second is what "share my screen"
    /// means, so the iOS implementation was deliberately not copied - see the note in
    /// <c>Platforms/iOS/Custom/ScreenCapture.cs</c> about its limit.</para>
    /// <para>The frame path is the same either way, and is the part worth having written once:
    /// a <c>CMSampleBuffer</c> becomes an <c>RTCCVPixelBuffer</c>, then an <c>RTCVideoFrame</c>,
    /// pushed through a capturer delegate into the track's <c>RTCVideoSource</c>.</para>
    /// </remarks>
    public static class ScreenCapture
    {
        static SCStream _stream;
        static StreamOutput _output;
        static Webrtc.RTCVideoCapturer _capturer;
        static Webrtc.IRTCVideoCapturerDelegate _sink;
        static string _screenTrackId;
        static long _frameCount;

        // How long to wait for the permission gate before giving up. Long enough for somebody to
        // find the dialog, short enough that a refusal does not look like a hung application.
        const int PermissionTimeoutMs = 45_000;

        /// <summary>
        /// Whether a track is the one carrying a shared screen.
        /// </summary>
        public static bool IsScreenTrack(string trackId) =>
            trackId is not null && trackId == _screenTrackId;

        /// <summary>
        /// Starts capturing the main display into <paramref name="videoTrack"/>.
        /// </summary>
        /// <remarks>
        /// The main display, without asking which. ScreenCaptureKit can enumerate displays,
        /// windows and applications, and choosing between them is a picker this library has no
        /// business drawing - the Windows implementation makes the same choice and says so.
        /// </remarks>
        /// <exception cref="InvalidOperationException">There is nothing to capture, or the stream refused to start.</exception>
        public static async Task StartAsync(IMediaStreamTrack videoTrack, int maxFrameRate)
        {
            var nativeTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            var source = nativeTrack?.Source
                ?? throw new ArgumentException(
                    "The track to capture into has no video source.", nameof(videoTrack));

            Echo("screen capture: asking for shareable content");

            // GetShareableContentAsync, not GetCurrentProcessShareableContentAsync. The latter
            // returns only this process's own windows, which would have reproduced exactly the
            // iOS limitation that ScreenCaptureKit was chosen to avoid - and it was what this
            // called first, on 2026-09-12.
            //
            // This is also the call that needs Screen Recording permission, and it does not
            // return until that is settled. With the await on the UI thread the window freezes
            // meanwhile, which reads as a crash from the outside - so it is raced against a
            // timeout rather than left to hang.
            var contentTask = SCShareableContent.GetShareableContentAsync();

            if (await Task.WhenAny(contentTask, Task.Delay(PermissionTimeoutMs)) != contentTask)
            {
                Reset();
                throw new InvalidOperationException(
                    "Screen recording permission was not granted. Allow it in System Settings " +
                    "under Privacy & Security > Screen Recording, then try again.");
            }

            var content = await contentTask;

            var display = content?.Displays?.FirstOrDefault()
                ?? throw new InvalidOperationException("No display is available to capture.");

            // initWithDisplay:excludingApplications:exceptingWindows: - the two-argument form
            // is not bound. Excluding nothing but this application's own windows would need the
            // running application, and excluding none at all is the simpler honest start.
            var filter = new SCContentFilter(display,
                Array.Empty<SCWindow>(), SCContentFilterOption.Exclude);

            var configuration = new SCStreamConfiguration
            {
                Width = (nuint)display.Width,
                Height = (nuint)display.Height,
                MinimumFrameInterval = CMTime.FromSeconds(
                    maxFrameRate <= 0 ? 1d / 15 : 1d / maxFrameRate, 600),
                ShowsCursor = true,
            };

            _sink = (Webrtc.IRTCVideoCapturerDelegate)source;
            _capturer = new Webrtc.RTCVideoCapturer();
            _screenTrackId = videoTrack.Id;
            _frameCount = 0;
            _output = new StreamOutput();

            _stream = new SCStream(filter, configuration, null);

            if (!_stream.AddStreamOutput(_output, SCStreamOutputType.Screen, null, out var addError))
            {
                Reset();
                throw new InvalidOperationException(
                    $"The screen capture output was refused: {addError?.LocalizedDescription}");
            }

            var started = new TaskCompletionSource<NSError>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            _stream.StartCapture(error => started.TrySetResult(error));

            // Waiting for the answer rather than assuming it - the iOS side of this library
            // learned that the hard way on 2026-09-12, reporting a share the system had never
            // begun because a timeout was read as success.
            var failure = await started.Task;

            if (failure is not null)
            {
                Reset();
                throw new InvalidOperationException(
                    $"Screen capture would not start: {failure.LocalizedDescription}");
            }

            Echo($"screen capture: running {display.Width}x{display.Height} track={_screenTrackId}");
        }

        /// <summary>
        /// Stops capturing, if this started it.
        /// </summary>
        /// <remarks>
        /// Safe to call when nothing is capturing: a stop button and a page teardown both reach
        /// here, and a second stop is not a caller error. Stopping the stream is what actually
        /// releases the display - closing the producer only stops sending it.
        /// </remarks>
        public static void Stop()
        {
            var stream = _stream;
            Reset();

            if (stream is null)
                return;

            stream.StopCapture(error =>
            {
                if (error is not null)
                    Echo($"stopping screen capture reported: {error.LocalizedDescription}");
            });
        }

        static void Reset()
        {
            _stream = null;
            _output = null;
            _sink = null;
            _capturer = null;
            _screenTrackId = null;
            _frameCount = 0;
        }

        static void Deliver(CMSampleBuffer sampleBuffer)
        {
            var sink = _sink;
            if (sink is null)
                return;

            try
            {
                if (sampleBuffer?.GetImageBuffer() is not CVPixelBuffer pixelBuffer)
                    return;

                var timestampNs = (long)(sampleBuffer.PresentationTimeStamp.Seconds * 1_000_000_000);

                if (++_frameCount == 1 || _frameCount % 150 == 0)
                    Echo($"screen capture: frame {_frameCount} " +
                        $"{pixelBuffer.Width}x{pixelBuffer.Height}");

                using var buffer = new Webrtc.RTCCVPixelBuffer(pixelBuffer);
                using var frame = new Webrtc.RTCVideoFrame(
                    buffer, Webrtc.RTCVideoRotation.RTCVideoRotation_0, timestampNs);

                sink.DidCaptureVideoFrame(_capturer, frame);
            }
            catch (Exception exception)
            {
                // Said and swallowed: this runs on ScreenCaptureKit's queue, and an exception
                // escaping into Objective-C terminates the process.
                Echo($"dropping a screen frame: {exception.GetType().Name}: {exception.Message}");
            }
        }

        /// <summary>
        /// Receives sample buffers from ScreenCaptureKit.
        /// </summary>
        sealed class StreamOutput : NSObject, ISCStreamOutput
        {
            public void DidOutputSampleBuffer(SCStream stream, CMSampleBuffer sampleBuffer,
                SCStreamOutputType type)
            {
                // Screen samples only. ScreenCaptureKit also delivers system and microphone
                // audio through this same callback, and neither belongs on a video track.
                if (type == SCStreamOutputType.Screen)
                    Deliver(sampleBuffer);
            }
        }

        static void Echo(string line) => Console.WriteLine($"######## {line}");
    }
}
