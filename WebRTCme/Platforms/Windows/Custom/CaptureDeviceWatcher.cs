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

    // The device set as of the last tick, so a change can be noticed rather than only a loss.
    // Null until the first tick, which is not the same as "no devices" - the first tick must not
    // look like everything just appeared.
    private static HashSet<string> _lastSeen;

    // How many MediaDevices are listening for a change. Kept as a count rather than a flag
    // because the timer's lifetime is the union of this and the watched tracks: either alone is
    // reason enough to keep enumerating, and neither may switch it off while the other wants it.
    private static int _changeListeners;

    /// <summary>
    /// Raised when a capture device appears or disappears.
    /// </summary>
    /// <remarks>
    /// Static, and <see cref="MediaDevices"/> forwards it to its own instance event. The polling
    /// is a process-wide cost and there is no sense paying it once per <see cref="MediaDevices"/>.
    /// </remarks>
    internal static event Action DevicesChanged;

    /// <summary>
    /// Starts and stops the polling that <see cref="DevicesChanged"/> needs.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="Watch"/> because the two want it at different times, and the
    /// device-change case is the one that wants it when nothing is being captured at all: the
    /// question "what cameras are there" is usually asked by something drawing a device list,
    /// which is not a thing a call is doing.
    /// </remarks>
    internal static void AddChangeListener()
    {
        Interlocked.Increment(ref _changeListeners);

        lock (_gate)
            _timer ??= new Timer(_ => Poll(), null, IntervalMs, IntervalMs);
    }

    internal static void RemoveChangeListener()
    {
        if (Interlocked.Decrement(ref _changeListeners) < 0)
            Interlocked.Exchange(ref _changeListeners, 0);

        StopTimerIfIdle();
    }

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
        if (!_watched.IsEmpty || Volatile.Read(ref _changeListeners) > 0)
            return;

        lock (_gate)
        {
            if (!_watched.IsEmpty || Volatile.Read(ref _changeListeners) > 0)
                return;

            _timer?.Dispose();
            _timer = null;

            // So the next run starts from "unknown" rather than from a list that may be months
            // stale, which would otherwise report every device as having just appeared.
            _lastSeen = null;
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

        // Before the per-track work, because a device appearing is news too and no track can be
        // waiting on it. The first tick only records - it has nothing to compare against, and
        // announcing the devices that were already there is not a change.
        var previous = _lastSeen;
        _lastSeen = present;

        if (previous is not null && !previous.SetEquals(present))
        {
            try
            {
                DevicesChanged?.Invoke();
            }
            catch (Exception)
            {
                // A listener that throws must not stop the track work below, which is the half
                // that keeps a call honest.
            }
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
