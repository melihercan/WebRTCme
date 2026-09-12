using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CoreMedia;
using CoreVideo;
using Foundation;
using ReplayKit;
using WebRTCme.iOS;

namespace WebRTCme
{
    /// <summary>
    /// Screen capture for <c>getDisplayMedia</c>, on ReplayKit.
    /// </summary>
    /// <remarks>
    /// <para><b>What this captures, and what it does not.</b> <c>RPScreenRecorder.StartCapture</c>
    /// records <i>this application's own content</i> and nothing else. It is not the system-wide
    /// screen share an iOS user gets from Control Centre - that requires a Broadcast Upload
    /// Extension, which is a second bundle with its own process, an App Group to reach it, and
    /// frames crossing a process boundary. Sharing from here therefore shows the far side this
    /// app's own window, which is genuinely useful for very little except proving the pipeline.
    /// </para>
    /// <para>The pipeline is the point. ReplayKit hands over <c>CMSampleBuffer</c>s; getting those
    /// into a WebRTC track - wrapped as <c>RTCVideoFrame</c>, pushed through a capturer delegate
    /// into the track's <c>RTCVideoSource</c> - is the part that is the same whichever end the
    /// frames come from. A broadcast extension replaces the source of the buffers and leaves the
    /// rest of this file alone.</para>
    /// <para>One capture at a time: <c>RPScreenRecorder</c> is a singleton and a second
    /// <c>StartCapture</c> fails rather than replacing the first.</para>
    /// </remarks>
    public static class ScreenCapture
    {
        // The capturer whose delegate is the track's video source. RTCVideoCapturer is not
        // subclassed here - it exists only to be the "who captured this" argument that
        // DidCaptureVideoFrame requires, and it holds the delegate that receives the frames.
        static Webrtc.RTCVideoCapturer _capturer;
        static Webrtc.IRTCVideoCapturerDelegate _sink;
        static string _screenTrackId;

        // The frame rate ReplayKit is asked to hold to. It delivers as fast as the screen changes,
        // which for a still screen is almost never and for a scrolling one is 60 - so frames are
        // dropped here rather than asking for a rate ReplayKit does not honour.
        static double _minFrameIntervalNs;
        static long _lastFrameNs;
        static long _frameCount;

        /// <summary>
        /// Whether a track is the one carrying a shared screen.
        /// </summary>
        public static bool IsScreenTrack(string trackId) =>
            trackId is not null && trackId == _screenTrackId;

        /// <summary>
        /// Starts recording this app's screen into <paramref name="videoTrack"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Recording is unavailable, already running, or ReplayKit refused to start.
        /// </exception>
        public static async Task StartAsync(IMediaStreamTrack videoTrack, int maxFrameRate)
        {
            var recorder = RPScreenRecorder.SharedRecorder;

            if (!recorder.Available)
                throw new InvalidOperationException(
                    "Screen recording is not available on this device.");

            if (recorder.Recording)
                throw new InvalidOperationException(
                    "This application is already recording the screen.");

            var nativeTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            var source = nativeTrack?.Source
                ?? throw new ArgumentException(
                    "The track to capture into has no video source.", nameof(videoTrack));

            _sink = (Webrtc.IRTCVideoCapturerDelegate)source;
            _capturer = new Webrtc.RTCVideoCapturer();
            _screenTrackId = videoTrack.Id;
            _minFrameIntervalNs = maxFrameRate <= 0 ? 0 : 1_000_000_000d / maxFrameRate;
            _lastFrameNs = 0;
            _frameCount = 0;

            // Microphone off: the call already has one, and ReplayKit's would be a second audio
            // source nothing consumes.
            recorder.MicrophoneEnabled = false;

            // Completed by ReplayKit either way: nil on success, an error on failure.
            var started = new TaskCompletionSource<NSError>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            Echo("screen capture: asking ReplayKit to start");

            recorder.StartCapture(OnSample, error =>
            {
                Echo($"screen capture: ReplayKit answered - " +
                    $"{(error is null ? "started" : error.LocalizedDescription)}");
                started.TrySetResult(error);
            });

            // Waiting for that answer rather than assuming it. This used to give up after two
            // seconds and call the silence success, on the reasoning that a successful start
            // never calls the handler - which is simply untrue: startCaptureWithHandler's
            // completion handler runs on success as well, with a nil error.
            //
            // The cost of being wrong was a share that the app reported as running and iOS had
            // never begun: the button read "Stop sharing", the far side got a tile, no frames
            // arrived, and there was no red recording indicator to contradict any of it.
            //
            // The wait is long because the first use shows a consent alert, and how long that
            // takes is a question about the person holding the phone.
            var outcome = await Task.WhenAny(started.Task, Task.Delay(ConsentTimeoutMs));

            if (outcome != started.Task)
            {
                Reset();
                throw new InvalidOperationException(
                    "Screen recording did not start: iOS never answered. If it asked for " +
                    "permission, the request went unanswered.");
            }

            if (started.Task.Result is NSError failure)
            {
                Reset();
                throw new InvalidOperationException(
                    $"Screen recording would not start: {failure.LocalizedDescription}");
            }

            // Said out loud because the interesting failure is a start that reports success and
            // records nothing - which is what this method used to do.
            Echo($"screen capture: running={recorder.Recording} track={_screenTrackId}");
        }

        // How long to wait for ReplayKit's answer, which on first use includes the person
        // answering a consent alert.
        const int ConsentTimeoutMs = 60_000;

        /// <summary>
        /// Stops recording, if this started it.
        /// </summary>
        /// <remarks>
        /// Safe to call when nothing is recording: both a stop button and a page teardown reach
        /// here, and a second stop is not a caller error.
        /// </remarks>
        public static void Stop()
        {
            var recorder = RPScreenRecorder.SharedRecorder;

            if (recorder.Recording)
            {
                recorder.StopCapture(error =>
                {
                    if (error is not null)
                        Echo($"stopping screen capture reported: {error.LocalizedDescription}");
                });
            }

            Reset();
        }

        static void Reset()
        {
            _sink = null;
            _capturer = null;
            _screenTrackId = null;
            _lastFrameNs = 0;
            _frameCount = 0;
        }

        /// <summary>
        /// Turns one ReplayKit sample into a WebRTC frame.
        /// </summary>
        /// <remarks>
        /// Video samples only. ReplayKit also delivers app and microphone audio through this same
        /// handler, and neither belongs on a video track.
        ///
        /// The buffer is not copied. <c>RTCCVPixelBuffer</c> retains the <c>CVPixelBuffer</c> for
        /// as long as the frame lives, and the encoder reads it from there - copying would cost a
        /// full-resolution memcpy per frame for nothing.
        /// </remarks>
        static void OnSample(CMSampleBuffer sampleBuffer, RPSampleBufferType type, NSError error)
        {
            if (type != RPSampleBufferType.Video || error is not null || _sink is null)
                return;

            try
            {
                if (sampleBuffer?.GetImageBuffer() is not CVPixelBuffer pixelBuffer)
                    return;

                var timestampNs = (long)(sampleBuffer.PresentationTimeStamp.Seconds * 1_000_000_000);

                // Dropped here rather than asked of ReplayKit, which does not take a frame rate.
                if (_minFrameIntervalNs > 0 && _lastFrameNs != 0 &&
                    timestampNs - _lastFrameNs < _minFrameIntervalNs)
                    return;

                _lastFrameNs = timestampNs;

                // The first frame and then sparingly: silence here is the symptom that matters,
                // and there is no other way to tell "ReplayKit is not delivering" from
                // "delivering frames this code is dropping".
                if (++_frameCount == 1 || _frameCount % 150 == 0)
                    Echo($"screen capture: frame {_frameCount} " +
                        $"{pixelBuffer.Width}x{pixelBuffer.Height} ts={timestampNs}");

                // The CVPixelBuffer goes straight in: this constructor wraps it in an
                // RTCCVPixelBuffer itself, so building one here only to hand it over would be a
                // second wrapper around the same pixels.
                using var frame = new Webrtc.RTCVideoFrame(
                    pixelBuffer, Webrtc.RTCVideoRotation.RTCVideoRotation_0, timestampNs);

                _sink.DidCaptureVideoFrame(_capturer, frame);
            }
            catch (Exception exception)
            {
                // Said and swallowed. This runs on ReplayKit's callback thread, and an exception
                // escaping into Objective-C terminates the process - which is how the Streams
                // stub took the app down on 2026-09-12.
                Echo($"dropping a screen frame: {exception.GetType().Name}: {exception.Message}");
            }
        }

        static void Echo(string line) => Console.WriteLine($"######## {line}");
    }
}
