using System;
using System.Collections.Concurrent;
using System.Threading;
using AVFoundation;
using Foundation;
using WebRTCme.MacCatalyst;

namespace WebRTCme
{
    /// <summary>
    /// Reports capture devices appearing and disappearing, and ends the tracks that lose theirs.
    /// </summary>
    /// <remarks>
    /// <para>Apple says both of these plainly and nothing was listening.
    /// <c>AVCaptureDeviceWasConnectedNotification</c> and its disconnected twin have always been
    /// there; <c>IMediaDevices.OnDeviceChange</c> was declared on this platform and never raised,
    /// and a local track whose camera was unplugged stayed <c>Live</c> for ever.</para>
    /// <para>One class for both, because they are one notification pair read two ways. A
    /// disconnect is news for anybody watching the device list, and it is the end of the road for
    /// a track that was capturing from that particular device.</para>
    /// <para>The device-loss half completes work already written elsewhere:
    /// <c>CallViewModel</c> has recovered a local track whose device died since 2026-09-12, on
    /// any platform that raises <see cref="IMediaStreamTrack.OnEnded"/>. Blazor raised it because
    /// the browser does, Windows because it polls enumeration, Android reopens the camera
    /// underneath the track instead. Apple raised nothing, so the recovery never ran here.</para>
    /// <para>Observers are registered only while something is listening: a process that never asks
    /// should not hold notification observers for its lifetime.</para>
    /// </remarks>
    internal static class CaptureDeviceWatcher
    {
        // Local tracks by the device they capture from. On this platform a track's id *is* the
        // device's UniqueID - see MediaStream.Create - which is what makes the match possible
        // without a second registry.
        static readonly ConcurrentDictionary<string, MediaStreamTrack> _watched = new();

        static readonly object _gate = new();
        static NSObject _connected;
        static NSObject _disconnected;
        static int _changeListeners;

        /// <summary>
        /// Raised when a capture device is connected or disconnected.
        /// </summary>
        internal static event Action DevicesChanged;

        /// <summary>
        /// Watches the device a local track captures from, so the track can be ended with it.
        /// </summary>
        internal static void Watch(MediaStreamTrack track)
        {
            if (track?.Id is null)
                return;

            _watched[track.Id] = track;
            EnsureObservers();
        }

        /// <summary>
        /// Stops watching a track's device.
        /// </summary>
        internal static void Forget(string trackId)
        {
            if (trackId is not null)
                _watched.TryRemove(trackId, out _);

            ReleaseObserversIfIdle();
        }

        internal static void AddChangeListener()
        {
            Interlocked.Increment(ref _changeListeners);
            EnsureObservers();
        }

        internal static void RemoveChangeListener()
        {
            if (Interlocked.Decrement(ref _changeListeners) < 0)
                Interlocked.Exchange(ref _changeListeners, 0);

            ReleaseObserversIfIdle();
        }

        static void EnsureObservers()
        {
            lock (_gate)
            {
                _connected ??= NSNotificationCenter.DefaultCenter.AddObserver(
                    AVCaptureDevice.WasConnectedNotification, OnConnected);

                _disconnected ??= NSNotificationCenter.DefaultCenter.AddObserver(
                    AVCaptureDevice.WasDisconnectedNotification, OnDisconnected);
            }
        }

        static void ReleaseObserversIfIdle()
        {
            if (!_watched.IsEmpty || Volatile.Read(ref _changeListeners) > 0)
                return;

            lock (_gate)
            {
                if (!_watched.IsEmpty || Volatile.Read(ref _changeListeners) > 0)
                    return;

                if (_connected is not null)
                    NSNotificationCenter.DefaultCenter.RemoveObserver(_connected);

                if (_disconnected is not null)
                    NSNotificationCenter.DefaultCenter.RemoveObserver(_disconnected);

                _connected = null;
                _disconnected = null;
            }
        }

        static void OnConnected(NSNotification notification) =>
            Raise($"connected {UniqueIdOf(notification) ?? "?"}");

        /// <summary>
        /// Ends any track that was capturing from the device that just went away, then reports
        /// the change.
        /// </summary>
        /// <remarks>
        /// The track first, because ending it is what the call depends on: a recovery is waiting
        /// on <c>OnEnded</c>, and a device-list refresh is not.
        /// </remarks>
        static void OnDisconnected(NSNotification notification)
        {
            var uniqueId = UniqueIdOf(notification);

            if (uniqueId is not null && _watched.TryRemove(uniqueId, out var track))
            {
                try
                {
                    // Stop() is what a track ending looks like from outside: disabled, Ended, and
                    // OnEnded raised. Whether that end was wanted is the listener's question -
                    // CallViewModel answers it by knowing whether it is tearing down.
                    track.Stop();
                }
                catch (Exception exception)
                {
                    Echo($"ending the track for {uniqueId} failed: {exception.Message}");
                }
            }

            Raise($"disconnected {uniqueId ?? "?"}");
        }

        static string UniqueIdOf(NSNotification notification) =>
            (notification?.Object as AVCaptureDevice)?.UniqueID;

        static void Raise(string what)
        {
            Echo($"devices changed: {what}");

            try
            {
                DevicesChanged?.Invoke();
            }
            catch (Exception exception)
            {
                // A listener that throws is its own problem. These arrive on a notification
                // thread, and letting one escape into Objective-C takes the process with it.
                Echo($"a device-change listener threw: {exception.Message}");
            }
        }

        static void Echo(string line) => Console.WriteLine($"######## {line}");
    }
}
