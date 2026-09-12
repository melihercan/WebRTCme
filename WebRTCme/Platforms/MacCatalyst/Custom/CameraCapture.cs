using System;
using System.Collections.Concurrent;
using AVFoundation;
using CoreFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using WebRTCme.MacCatalyst;

namespace WebRTCme
{
    /// <summary>
    /// Camera capture for Mac Catalyst, driven from an <see cref="AVCaptureSession"/> this library
    /// owns rather than from libwebrtc's <c>RTCCameraVideoCapturer</c>.
    /// </summary>
    /// <remarks>
    /// <para>The reason is rotation, and it is worth writing down because the symptom pointed away
    /// from the cause for most of a day.</para>
    /// <para><c>RTCCameraVideoCapturer</c> tags every frame it produces with a rotation derived
    /// from <c>UIDevice.orientation</c>. On an iPhone that is exactly right. On a Mac there is no
    /// device orientation - the value describes nothing - so frames left this machine tagged for a
    /// quarter turn the scene never had, and every peer dutifully applied it. Seen on 2026-09-12:
    /// an upright webcam, an upright picture in the Mac's own window, and the same stream arriving
    /// sideways on an Android peer.</para>
    /// <para>The Mac's own window looked right for a reason that hid the bug rather than
    /// contradicting it: for a camera track <c>MediaView</c> renders an
    /// <c>RTCCameraPreviewView</c> over the capture session, so the self-view is the AVFoundation
    /// preview layer and never touches a WebRTC frame. It was upright because the camera is
    /// upright. It said nothing about what was being sent.</para>
    /// <para>So the frames are built here instead: a <c>CMSampleBuffer</c> becomes an
    /// <c>RTCCVPixelBuffer</c>, then an <c>RTCVideoFrame</c> with
    /// <c>RTCVideoRotation_0</c>, pushed through the capturer delegate into the track's video
    /// source. That is the same path <c>ScreenCapture</c> already uses on this platform, which is
    /// why it is the shape chosen - it is known to work here.</para>
    /// <para>Rotation 0 is not a guess and not a workaround: a camera wired to a Mac does not
    /// move, so upright is the truth about every frame it produces. The platforms that genuinely
    /// rotate - iOS, Android - are deliberately left alone.</para>
    /// </remarks>
    internal static class CameraCapture
    {
        // One session per track. Binding a track to a view runs the whole setup again - a tile
        // rebuild is enough - and a second AVCaptureSession on a camera that is already capturing
        // fails at StartRunning. The same guard exists on Android, for the same reason.
        static readonly ConcurrentDictionary<string, Capture> _capturesByTrackId = new();

        /// <summary>
        /// Starts capturing <paramref name="device"/> into <paramref name="videoTrack"/>, and
        /// returns the session so a preview view can show it.
        /// </summary>
        internal static AVCaptureSession Start(IMediaStreamTrack videoTrack, AVCaptureDevice device,
            AVCaptureDeviceFormat format, int frameRate)
        {
            var nativeTrack = ((MediaStreamTrack)videoTrack).NativeObject as Webrtc.RTCVideoTrack;
            var source = nativeTrack?.Source
                ?? throw new ArgumentException(
                    "The track to capture into has no video source.", nameof(videoTrack));

            var capture = _capturesByTrackId.GetOrAdd(videoTrack.Id,
                _ => Open(device, format, frameRate, (Webrtc.IRTCVideoCapturerDelegate)source));

            return capture.Session;
        }

        /// <summary>
        /// Stops capturing for a track, if this started it. Safe to call when nothing is running.
        /// </summary>
        internal static void Stop(string trackId)
        {
            if (trackId is null || !_capturesByTrackId.TryRemove(trackId, out var capture))
                return;

            try
            {
                capture.Session.StopRunning();
                Echo($"camera capture stopped for {trackId}");
            }
            catch (Exception exception)
            {
                Echo($"stopping the camera for {trackId} reported: {exception.Message}");
            }
        }

        static Capture Open(AVCaptureDevice device, AVCaptureDeviceFormat format, int frameRate,
            Webrtc.IRTCVideoCapturerDelegate sink)
        {
            var session = new AVCaptureSession();
            session.BeginConfiguration();

            // Input priority, and this is not optional: with any other preset the session owns the
            // format, and setting device.ActiveFormat underneath it gets overridden when the
            // session starts. Configured the other way on 2026-09-12 the camera delivered its
            // first frame and then a trickle - the session and the device disagreeing about the
            // format for the rest of the call.
            if (session.CanSetSessionPreset(AVCaptureSession.PresetInputPriority))
                session.SessionPreset = AVCaptureSession.PresetInputPriority;

            var input = AVCaptureDeviceInput.FromDevice(device, out var inputError);
            if (input is null)
                throw new InvalidOperationException(
                    $"Could not open camera '{device.UniqueID}': {inputError?.LocalizedDescription}");

            if (!session.CanAddInput(input))
                throw new InvalidOperationException(
                    $"Camera '{device.UniqueID}' cannot be added to a capture session.");

            session.AddInput(input);

            var output = new AVCaptureVideoDataOutput
            {
                // Bi-planar NV12, which is what RTCCVPixelBuffer expects; anything else would be
                // converted frame by frame, or rejected.
                WeakVideoSettings = new AVVideoSettingsUncompressed
                {
                    PixelFormatType = CVPixelFormatType.CV420YpCbCr8BiPlanarFullRange,
                }.Dictionary,

                // A late frame in a live call is worth less than the next one.
                AlwaysDiscardsLateVideoFrames = true,
            };

            var delegateObject = new Frames(sink);
            var queue = new DispatchQueue("WebRTCme.CameraCapture");
            output.SetSampleBufferDelegate(delegateObject, queue);

            if (!session.CanAddOutput(output))
                throw new InvalidOperationException(
                    $"Camera '{device.UniqueID}' cannot deliver sample buffers to this session.");

            session.AddOutput(output);

            // Inside the transaction, so the session sees one consistent configuration rather
            // than a format that changes under it after it has committed.
            ApplyFormat(device, format, frameRate);

            session.CommitConfiguration();
            session.StartRunning();

            var dimensions = ((CMVideoFormatDescription)format.FormatDescription).Dimensions;
            Echo($"camera capture running {dimensions.Width}x{dimensions.Height}@{frameRate} " +
                 $"device={device.UniqueID}");

            // Every piece is held: AVFoundation keeps only weak references to the delegate and the
            // queue, and a collected delegate is a camera that runs and delivers nothing.
            return new Capture(session, input, output, delegateObject, queue);
        }

        /// <summary>
        /// Puts the chosen format and frame rate on the device.
        /// </summary>
        /// <remarks>
        /// Best effort. A camera that refuses the format still captures at whatever it was already
        /// set to, and a call with slightly the wrong resolution beats no call at all.
        /// </remarks>
        static void ApplyFormat(AVCaptureDevice device, AVCaptureDeviceFormat format, int frameRate)
        {
            if (!device.LockForConfiguration(out var lockError))
            {
                Echo($"could not lock {device.UniqueID} to set its format: " +
                     $"{lockError?.LocalizedDescription}");
                return;
            }

            try
            {
                device.ActiveFormat = format;

                if (frameRate > 0)
                {
                    var duration = new CMTime(1, frameRate);
                    device.ActiveVideoMinFrameDuration = duration;
                    device.ActiveVideoMaxFrameDuration = duration;
                }
            }
            catch (Exception exception)
            {
                Echo($"setting the format on {device.UniqueID} failed: {exception.Message}");
            }
            finally
            {
                device.UnlockForConfiguration();
            }
        }

        /// <summary>
        /// Everything one running capture needs kept alive.
        /// </summary>
        sealed record Capture(
            AVCaptureSession Session,
            AVCaptureDeviceInput Input,
            AVCaptureVideoDataOutput Output,
            Frames Delegate,
            DispatchQueue Queue);

        /// <summary>
        /// Turns AVFoundation sample buffers into WebRTC frames.
        /// </summary>
        sealed class Frames : AVCaptureVideoDataOutputSampleBufferDelegate
        {
            readonly Webrtc.IRTCVideoCapturerDelegate _sink;

            // The capturer argument is only an identity for the delegate call; nothing downstream
            // reads it, and one instance for the lifetime of the capture is enough.
            readonly Webrtc.RTCVideoCapturer _capturer = new();

            long _frameCount;

            internal Frames(Webrtc.IRTCVideoCapturerDelegate sink) => _sink = sink;

            public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput,
                CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
            {
                try
                {
                    if (sampleBuffer?.GetImageBuffer() is not CVPixelBuffer pixelBuffer)
                        return;

                    var timestampNs =
                        (long)(sampleBuffer.PresentationTimeStamp.Seconds * 1_000_000_000);

                    // Every 60 frames - two seconds at 30fps. Frequent enough that a stall shows
                    // up as a gap in the timestamps rather than as silence.
                    if (++_frameCount == 1 || _frameCount % 60 == 0)
                        Echo($"camera frame {_frameCount} {pixelBuffer.Width}x{pixelBuffer.Height}");

                    using var buffer = new Webrtc.RTCCVPixelBuffer(pixelBuffer);
                    using var frame = new Webrtc.RTCVideoFrame(
                        buffer, Webrtc.RTCVideoRotation.RTCVideoRotation_0, timestampNs);

                    _sink.DidCaptureVideoFrame(_capturer, frame);
                }
                catch (Exception exception)
                {
                    // Said and swallowed: this runs on AVFoundation's queue, and an exception
                    // escaping into Objective-C terminates the process.
                    Echo($"dropping a camera frame: {exception.GetType().Name}: {exception.Message}");
                }
                finally
                {
                    // AVCaptureVideoDataOutput reuses its buffers and will stall once its pool is
                    // exhausted, so each sample is released as soon as it has been copied out.
                    sampleBuffer?.Dispose();
                }
            }
        }

        static void Echo(string line) => Console.WriteLine($"######## {line}");
    }
}
