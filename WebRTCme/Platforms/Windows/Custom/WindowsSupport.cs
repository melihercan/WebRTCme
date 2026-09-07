using WebRTCme.Windows;
using static WebRTCme.Bindings.Maui.Windows.Interop;

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

    /// <summary>
    /// Lists what can be shared. The W3C <c>getDisplayMedia</c> shows the browser's own picker
    /// and never exposes this, so an app that wants to offer a choice has to come through here.
    /// </summary>
    public static IReadOnlyList<DesktopSource> GetDesktopSources(DesktopSourceKind kind)
    {
        var abiKind = kind == DesktopSourceKind.Window ? DesktopSourceWindow : DesktopSourceScreen;

        WebRtcRuntime.Check(DesktopSourceCount(abiKind, out var count),
                            $"count shareable {kind}s");

        var sources = new List<DesktopSource>(count);
        for (var index = 0; index < count; index++)
        {
            if (DesktopSourceInfo(abiKind, index, out var title, out var id) != Ok)
                continue;

            sources.Add(new DesktopSource(id, WebRtcRuntime.TakeString(title), kind));
        }

        return sources;
    }

    /// <summary>Captures one screen or window as a stream carrying a single video track.</summary>
    /// <param name="maxFrameRate">Screen content is read rather than watched, so a modest rate
    /// leaves bandwidth for the resolution that keeps text legible.</param>
    public static IMediaStream GetDisplayMedia(DesktopSource source, int maxFrameRate = 15)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxFrameRate, 0);

        var abiKind = source.Kind == DesktopSourceKind.Window
            ? DesktopSourceWindow
            : DesktopSourceScreen;

        var track = MediaDevices.CreateDesktopTrack(abiKind, source.Id, source.Title, maxFrameRate);
        return new WebRTCme.Windows.MediaStream([track]);
    }
}

public enum DesktopSourceKind
{
    Screen,
    Window
}

/// <summary>Something that can be shared. <see cref="Id"/> identifies it, not its position in
/// the list, which is not stable across calls.</summary>
public sealed record DesktopSource(long Id, string Title, DesktopSourceKind Kind);
