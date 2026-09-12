using System;
using Foundation;
using ObjCRuntime;

namespace WebRTCme
{
    /// <summary>
    /// Corrects the rotation libwebrtc attaches to camera frames on Mac Catalyst.
    /// </summary>
    /// <remarks>
    /// <para><c>RTCCameraVideoCapturer</c> tags every frame with a rotation derived from
    /// <c>UIDevice.orientation</c>. On an iPhone that is exactly right. On a Mac there is no device
    /// orientation - the value describes nothing - so frames left this machine tagged for a quarter
    /// turn the scene never had, and every peer applied it. Seen on 2026-09-12: an upright webcam,
    /// an upright picture in the Mac's own window, and the same stream arriving sideways on
    /// Android.</para>
    /// <para>The Mac's own window looked right for a reason that hid the fault rather than
    /// contradicting it: for a camera track <c>MediaView</c> renders an
    /// <c>RTCCameraPreviewView</c> over the capture session, so the self-view is the AVFoundation
    /// preview layer and never touches a WebRTC frame. It was upright because the camera is
    /// upright, and said nothing about what was being sent.</para>
    /// <para><b>This sits between the capturer and the video source and changes one field.</b> The
    /// first attempt replaced <c>RTCCameraVideoCapturer</c> with an <c>AVCaptureSession</c> this
    /// library drove itself, building each frame from a <c>CMSampleBuffer</c> the way
    /// <c>ScreenCapture</c> does. It produced upright video that froze - frames arrived at a
    /// fraction of the rate the capturer manages, and two rounds of correcting the session
    /// configuration did not recover it. Reimplementing capture in order to change a rotation was
    /// the wrong trade: the capturer works, and only the label on its output was wrong.</para>
    /// <para>Rotation 0 is the truth rather than a workaround: a camera wired to a Mac does not
    /// move. iOS and Android genuinely rotate, so neither gets this.</para>
    /// </remarks>
    internal static class CameraCapture
    {
        /// <summary>
        /// Wraps a video source so camera frames reach it upright.
        /// </summary>
        internal static Webrtc.IRTCVideoCapturerDelegate Upright(
            Webrtc.IRTCVideoCapturerDelegate sink) => new UprightFrames(sink);

        sealed class UprightFrames : NSObject, Webrtc.IRTCVideoCapturerDelegate
        {
            readonly Webrtc.IRTCVideoCapturerDelegate _sink;
            long _frameCount;
            bool _saidWhatItIsDoing;

            internal UprightFrames(Webrtc.IRTCVideoCapturerDelegate sink) => _sink = sink;

            [Export("capturer:didCaptureVideoFrame:")]
            public void DidCaptureVideoFrame(Webrtc.RTCVideoCapturer capturer,
                Webrtc.RTCVideoFrame frame)
            {
                try
                {
                    if (frame is null)
                        return;

                    if (frame.Rotation == Webrtc.RTCVideoRotation.RTCVideoRotation_0)
                    {
                        // Nothing to correct, so nothing is rebuilt.
                        _sink.DidCaptureVideoFrame(capturer, frame);
                        return;
                    }

                    // The frame's buffer is typed as the generated RTCVideoFrameBuffer and the
                    // constructor wants the concrete RTCCVPixelBuffer - the binding cannot express
                    // the protocol conformance between them, which is recorded beside that
                    // constructor in ApiDefinitions. Same native object either way, re-wrapped as
                    // the type the constructor accepts. owns: false because the frame owns it.
                    var buffer = Runtime.GetINativeObject<Webrtc.RTCCVPixelBuffer>(
                        frame.Buffer.Handle, forced_type: true, owns: false);

                    if (buffer is null)
                    {
                        // Not a pixel buffer, so not something this can rebuild. Sideways video
                        // beats none.
                        _sink.DidCaptureVideoFrame(capturer, frame);
                        return;
                    }

                    if (!_saidWhatItIsDoing)
                    {
                        _saidWhatItIsDoing = true;
                        Echo($"correcting camera rotation: {frame.Rotation} -> 0 " +
                             $"({frame.Width}x{frame.Height})");
                    }

                    using var upright = new Webrtc.RTCVideoFrame(
                        buffer, Webrtc.RTCVideoRotation.RTCVideoRotation_0, frame.TimeStampNs);

                    _sink.DidCaptureVideoFrame(capturer, upright);

                    if (++_frameCount % 300 == 0)
                        Echo($"camera frame {_frameCount} upright");
                }
                catch (Exception exception)
                {
                    // Swallowed, and said rarely: this runs on the capturer's queue, an exception
                    // escaping into Objective-C would end the process, and a fault here would
                    // otherwise repeat thirty times a second.
                    if (_frameCount++ % 300 == 0)
                        Echo("correcting a camera frame failed: " +
                             $"{exception.GetType().Name}: {exception.Message}");
                }
            }
        }

        static void Echo(string line)
        {
            Console.WriteLine($"######## {line}");

            // Flushed, because this log is read to decide whether frames are arriving and a
            // buffered stream makes "no frames" and "not flushed yet" look the same.
            try { Console.Out.Flush(); } catch { }
        }
    }
}
