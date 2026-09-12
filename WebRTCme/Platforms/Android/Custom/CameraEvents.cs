using System;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Platforms.Android.Custom
{
    /// <summary>
    /// Listens to what a camera capturer has to say about its camera.
    /// </summary>
    /// <remarks>
    /// This used to be null, which is why a camera that went away stayed away. Android hands the
    /// camera to a higher-priority client whenever it wants one - face unlock on the lock screen
    /// is the common case, the system camera app is another - and libwebrtc's capturer responds by
    /// closing its session and saying so here. It does not reopen: reopening is the application's
    /// business, because only the application knows whether the call is still wanted.
    ///
    /// Measured on 2026-09-12, with the call otherwise healthy and backgrounded for two minutes:
    /// waking the phone ran face unlock, which took camera 1, and the capture ended with
    /// "Camera device closed". Outgoing video froze at 7924 frames while audio kept flowing, so
    /// the call looked alive at both ends with a still picture on each.
    /// </remarks>
    internal class CameraEvents : Java.Lang.Object, Webrtc.ICameraVideoCapturer.ICameraEventsHandler
    {
        readonly string _trackId;

        public CameraEvents(string trackId) => _trackId = trackId;

        // Both mean the session is gone. Disconnected is another client taking the camera; error
        // is everything else, including failing to open it - which is what a retry hits while the
        // other client still holds it, so it has to drive the retry as well.
        public void OnCameraDisconnected() => AndroidSupport.CameraLost(_trackId, "disconnected");

        public void OnCameraError(string errorDescription) =>
            AndroidSupport.CameraLost(_trackId, errorDescription ?? "error");

        // Not treated as a loss. A freeze is the capturer reporting that frames have stopped
        // arriving on a session it still holds, and tearing that session down to build another
        // would as easily make things worse. Said out loud so it is visible if it turns out to be
        // a separate cause of the same symptom.
        public void OnCameraFreezed(string errorDescription) =>
            AndroidSupport.CameraEcho($"camera {_trackId} froze: {errorDescription}");

        public void OnCameraOpening(string cameraName) =>
            AndroidSupport.CameraEcho($"camera {cameraName} opening");

        // The two signals that a restart actually worked, rather than being accepted and then
        // failing asynchronously - see AndroidSupport.CameraLost.
        public void OnFirstFrameAvailable() => AndroidSupport.CameraRecovered(_trackId);

        public void OnCameraClosed() => AndroidSupport.CameraEcho($"camera {_trackId} closed");
    }
}
