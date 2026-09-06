using WebRTCme.Windows;

namespace WebRTCme;

/// <summary>
/// Bridge for platform code living outside this assembly -- today the MAUI middleware's
/// MediaView. Mirrors the role AndroidSupport plays for the Android binding.
/// </summary>
public static class WindowsSupport
{
    /// <summary>
    /// Subscribes to frames from a video track and delivers them as tightly packed BGRA8, which
    /// is what a WinUI <c>WriteableBitmap</c> expects. Remote tracks deliver frames decoded from
    /// the peer; local tracks deliver camera frames, giving a self-preview.
    /// </summary>
    /// <remarks>
    /// Frames arrive on a WebRTC capture or decode thread, one call per frame, and the handler
    /// runs before the next frame is converted -- so anything slower than the frame interval
    /// costs frames. Hand off and return.
    /// </remarks>
    /// <returns>A subscription that detaches the handler when disposed.</returns>
    public static IDisposable SubscribeToVideoFrames(IMediaStreamTrack videoTrack,
                                                     Action<byte[], int, int> onBgraFrame)
    {
        ArgumentNullException.ThrowIfNull(videoTrack);
        ArgumentNullException.ThrowIfNull(onBgraFrame);

        if (videoTrack is not MediaStreamTrack track)
            throw new ArgumentException(
                $"Track must come from the Windows binding, got {videoTrack.GetType().FullName}.",
                nameof(videoTrack));

        return track.SubscribeToFrames(onBgraFrame);
    }
}
