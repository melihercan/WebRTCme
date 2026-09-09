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
            if (trackId is null || !_capturersByTrackId.TryRemove(trackId, out var videoCapturer))
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
                    videoCapturer.StartCapture(480, 640, 30);
                    return videoCapturer;
                });
            }

            nativeVideoTrack.AddSink(rendererView);
        }
    }
}

