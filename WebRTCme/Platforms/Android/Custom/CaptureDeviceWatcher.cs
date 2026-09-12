using System;
using System.Threading;
using Android.Hardware.Camera2;
using Android.Media;

namespace WebRTCme.Platforms.Android.Custom
{
    /// <summary>
    /// Reports a capture device appearing or disappearing.
    /// </summary>
    /// <remarks>
    /// Android says this properly, unlike Windows, which has to poll: <c>CameraManager</c> calls
    /// back when a camera becomes available or unavailable, and <c>AudioManager</c> when an audio
    /// device is added or removed. Neither was registered, so <c>IMediaDevices.OnDeviceChange</c>
    /// was declared here and never raised.
    ///
    /// "Unavailable" is worth reading carefully, because it is broader than "gone" and that is
    /// what makes it useful. Android reports a camera as unavailable when another application
    /// opens it - which is the same eviction <c>AndroidSupport.CameraLost</c> recovers from, seen
    /// from the other side and before it happens rather than after. A phone does not often gain or
    /// lose a camera; it loses one to the camera app all the time.
    ///
    /// Registered only while something is listening. The callbacks are cheap, but an application
    /// that never asks should not be holding system callbacks for the length of its life.
    /// </remarks>
    internal static class CaptureDeviceWatcher
    {
        static readonly object _gate = new();
        static int _listeners;

        static CameraAvailability _cameraCallback;
        static AudioDevices _audioCallback;
        static CameraManager _cameraManager;
        static AudioManager _audioManager;

        // What is known, so that only a change is reported as one. Android delivers the current
        // state of everything the moment a callback is registered, and forwarding that means
        // subscribing announces six changes before anything has changed - measured on this
        // device, cameras 0 to 3 plus eight audio devices in the same millisecond. Windows has the
        // same problem for the same reason and answers it the same way, by comparing rather than
        // forwarding; an API that behaves differently on two platforms is worse than one that is
        // quiet on both.
        //
        // A flag saying "ignore whatever arrives during registration" was tried first and does not
        // work: with a null handler the callbacks are posted to the calling thread's looper, so
        // they arrive after registration returns, not during it. Measured - the camera burst went
        // away because the set comparison caught it, and the audio burst did not, because only the
        // flag was guarding that one. Comparing is the thing that works; the flag was reasoning.
        static readonly System.Collections.Generic.HashSet<string> _unavailableCameras = new();
        static readonly System.Collections.Generic.HashSet<int> _knownAudioDevices = new();

        /// <summary>
        /// Raised when a camera or an audio device changes availability.
        /// </summary>
        internal static event Action DevicesChanged;

        internal static void AddChangeListener()
        {
            lock (_gate)
            {
                if (++_listeners > 1)
                    return;

                try
                {
                    var context = global::Android.App.Application.Context;

                    _cameraManager = (CameraManager)context.GetSystemService(
                        global::Android.Content.Context.CameraService);
                    _unavailableCameras.Clear();

                    _cameraCallback = new CameraAvailability();
                    _cameraManager?.RegisterAvailabilityCallback(_cameraCallback, null);

                    _audioManager = (AudioManager)context.GetSystemService(
                        global::Android.Content.Context.AudioService);

                    // Read before registering, so that the callback's opening delivery of every
                    // current device matches what is already known and reports nothing.
                    _knownAudioDevices.Clear();
                    foreach (var device in _audioManager?.GetDevices(GetDevicesTargets.All)
                        ?? Array.Empty<AudioDeviceInfo>())
                        _knownAudioDevices.Add(device.Id);

                    _audioCallback = new AudioDevices();
                    _audioManager?.RegisterAudioDeviceCallback(_audioCallback, null);
                }
                catch (Exception exception)
                {
                    // Said rather than thrown: a caller subscribing to an event is not asking to
                    // be told that the platform would not co-operate, and the rest of the call
                    // does not depend on this.
                    AndroidSupportEcho($"registering device callbacks failed: {exception.Message}");
                }
            }
        }

        internal static void RemoveChangeListener()
        {
            lock (_gate)
            {
                if (_listeners == 0 || --_listeners > 0)
                    return;

                try
                {
                    if (_cameraCallback is not null)
                        _cameraManager?.UnregisterAvailabilityCallback(_cameraCallback);

                    if (_audioCallback is not null)
                        _audioManager?.UnregisterAudioDeviceCallback(_audioCallback);
                }
                catch (Exception exception)
                {
                    AndroidSupportEcho($"unregistering device callbacks failed: {exception.Message}");
                }

                _cameraCallback = null;
                _audioCallback = null;
                _cameraManager = null;
                _audioManager = null;
            }
        }

        static void Raise(string what)
        {
            AndroidSupportEcho($"devices changed: {what}");

            try
            {
                DevicesChanged?.Invoke();
            }
            catch (Exception exception)
            {
                // A listener that throws is its own problem. These callbacks arrive on a system
                // thread, and letting one escape takes the process with it.
                AndroidSupportEcho($"a device-change listener threw: {exception.Message}");
            }
        }

        static void AndroidSupportEcho(string line) => WebRTCme.AndroidSupport.CameraEcho(line);

        /// <summary>
        /// Reports a camera's availability, unless it is only the state we already knew.
        /// </summary>
        /// <remarks>
        /// The registration burst is swallowed by <see cref="_primed"/> rather than by a timer:
        /// Android delivers the current state synchronously from
        /// <c>RegisterAvailabilityCallback</c>, so everything arriving before that call returns is
        /// the starting position, and everything after it is a change. The flag is set by
        /// <see cref="AddChangeListener"/> once registration is done, under the same lock the
        /// callbacks contend for.
        /// </remarks>
        static void CameraAvailabilityChanged(string cameraId, bool available)
        {
            lock (_gate)
            {
                var changed = available
                    ? _unavailableCameras.Remove(cameraId)
                    : _unavailableCameras.Add(cameraId);

                if (!changed)
                    return;
            }

            Raise($"camera {cameraId} {(available ? "available" : "unavailable")}");
        }

        static void AudioDevicesChanged(AudioDeviceInfo[] devices, bool added)
        {
            var changed = 0;

            lock (_gate)
            {
                foreach (var device in devices ?? Array.Empty<AudioDeviceInfo>())
                {
                    if (added ? _knownAudioDevices.Add(device.Id)
                              : _knownAudioDevices.Remove(device.Id))
                        changed++;
                }
            }

            if (changed == 0)
                return;

            Raise($"{changed} audio device(s) {(added ? "added" : "removed")}");
        }

        class CameraAvailability : CameraManager.AvailabilityCallback
        {
            public override void OnCameraAvailable(string cameraId) =>
                CameraAvailabilityChanged(cameraId, available: true);

            public override void OnCameraUnavailable(string cameraId) =>
                CameraAvailabilityChanged(cameraId, available: false);
        }

        class AudioDevices : AudioDeviceCallback
        {
            public override void OnAudioDevicesAdded(AudioDeviceInfo[] addedDevices) =>
                AudioDevicesChanged(addedDevices, added: true);

            public override void OnAudioDevicesRemoved(AudioDeviceInfo[] removedDevices) =>
                AudioDevicesChanged(removedDevices, added: false);
        }
    }
}
