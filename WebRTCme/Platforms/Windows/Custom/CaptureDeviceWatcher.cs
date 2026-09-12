using System.Collections.Concurrent;

namespace WebRTCme.Windows;

/// <summary>
/// Ends a local track whose capture device has gone away.
/// </summary>
/// <remarks>
/// Every other platform here is told. A browser fires <c>ended</c>, Android's capturer reports
/// <c>onCameraDisconnected</c>, AVFoundation posts an interruption notification. The interop shim
/// reports nothing at all - its ABI has device enumeration and no device-lost callback - so
/// unplugging a webcam mid-call left a track that was still <c>Live</c>, still attached to its
/// sender, and delivering nothing. The far side keeps a frozen tile on a call that says it is
/// connected, which is the same silent failure this has now been chased through three layers.
///
/// So: enumeration, on a timer, compared against the devices the live local tracks were opened
/// with. Polling is not the answer anyone wants, and it is the only one available without a
/// callback in <c>WebRtcInterop.dll</c>, whose source is not in this repository. It is kept as
/// cheap as it can be - the timer exists only while something is being captured, two seconds
/// apart, and enumeration is a handful of P/Invokes that touch no media path.
///
/// A track is ended rather than repaired here. Reopening is a decision about the call, not about
/// a device, and the layer that owns it is <c>CallViewModel</c>, which is already listening for
/// exactly this.
/// </remarks>
internal static class CaptureDeviceWatcher
{
    private const int IntervalMs = 2000;

    private static readonly ConcurrentDictionary<MediaStreamTrack, byte> _watched = new();
    private static readonly object _gate = new();
    private static Timer _timer;

    /// <summary>
    /// Starts watching the device a local track was opened with.
    /// </summary>
    /// <remarks>
    /// Local tracks with a device id only. A remote track has no device here, and a desktop track
    /// is created with a null device id because a screen is not enumerated alongside cameras -
    /// losing a screen is a different event, reported by the capturer rather than missed from a
    /// list.
    /// </remarks>
    internal static void Watch(MediaStreamTrack track)
    {
        if (track is null || track.IsRemote || string.IsNullOrEmpty(track.DeviceId))
            return;

        _watched[track] = 0;

        lock (_gate)
            _timer ??= new Timer(_ => Poll(), null, IntervalMs, IntervalMs);
    }

    /// <summary>
    /// Stops watching a track, and stops the timer once nothing is left to watch.
    /// </summary>
    internal static void Forget(MediaStreamTrack track)
    {
        if (track is not null)
            _watched.TryRemove(track, out _);

        StopTimerIfIdle();
    }

    private static void StopTimerIfIdle()
    {
        if (!_watched.IsEmpty)
            return;

        lock (_gate)
        {
            if (!_watched.IsEmpty)
                return;

            _timer?.Dispose();
            _timer = null;
        }
    }

    private static void Poll()
    {
        HashSet<string> present;

        try
        {
            present = PresentInputDeviceIds();
        }
        catch (Exception)
        {
            // Enumeration failing is not evidence that a device has gone, and acting on it would
            // end every track on the call. Skip this tick.
            return;
        }

        foreach (var track in _watched.Keys)
        {
            if (present.Contains(track.DeviceId))
                continue;

            _watched.TryRemove(track, out _);

            try
            {
                // Stop() is what a track ending looks like from the outside: disabled, Ended, and
                // OnEnded raised. It is not "the user stopped this" - who did is the listener's
                // question, and CallViewModel answers it by knowing whether it is tearing down.
                track.Stop();
            }
            catch (Exception)
            {
                // A track already disposed underneath us. Nothing to end.
            }
        }

        StopTimerIfIdle();
    }

    private static HashSet<string> PresentInputDeviceIds()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        foreach (var device in MediaDevices.EnumerateInputDevices())
            ids.Add(device);

        return ids;
    }
}
