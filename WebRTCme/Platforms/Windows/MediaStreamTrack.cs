using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static WebRTCme.Bindings.Maui.Windows.Interop;
using Frame = WebRTCme.Bindings.Maui.Windows.Interop.VideoFrame;

namespace WebRTCme.Windows;

/// <summary>
/// A track backed by an <c>rtc_media_track</c> handle, whether captured locally or arriving from
/// a peer.
/// </summary>
/// <remarks>
/// <para>The ABI allows a single frame sink per track, so this owns that one sink and fans frames
/// out to however many subscribers the app attaches -- a local track feeding both a self-preview
/// and something else would otherwise fail with INVALID_STATE on the second attach.</para>
/// <para>The frame thread never takes <see cref="_gate"/>. <c>rtc_video_track_remove_sink</c> is a
/// blocking call onto the thread that delivers frames, and it is made while the gate is held; a
/// frame callback that waited for the gate would then wait for the thread that is waiting for it.
/// That deadlock was seen in the wild (2026-09-19): a MAUI page navigating away from a preview
/// disposed its subscription with a frame in flight, and the UI thread and the capture thread
/// froze each other. So the subscriber list is copy-on-write: writers replace the array under
/// the gate, and <see cref="OnFrame"/> reads whichever array is current without locking.</para>
/// </remarks>
internal sealed class MediaStreamTrack : IMediaStreamTrack
{
    private static readonly Action<byte[], int, int>[] NoSubscribers = [];

    /// <summary>Replaced, never mutated: <see cref="OnFrame"/> reads it without the gate.</summary>
    private Action<byte[], int, int>[] _subscribers = NoSubscribers;
    private readonly object _gate = new();

    private IntPtr _handle;
    private GCHandle _self;
    private bool _sinkAttached;
    private bool _enabled = true;
    private MediaStreamTrackState _readyState = MediaStreamTrackState.Live;

    internal MediaStreamTrack(IntPtr handle, MediaStreamTrackKind kind, string id, string label,
                              bool isRemote, string deviceId = null,
                              int width = 0, int height = 0, int frameRate = 0)
    {
        _handle = handle;
        Kind = kind;
        Id = id;
        Label = label;
        IsRemote = isRemote;
        DeviceId = deviceId;
        Width = width;
        Height = height;
        FrameRate = frameRate;
    }

    internal IntPtr Handle => _handle;

    internal bool IsRemote { get; }

    internal string DeviceId { get; }

    internal int Width { get; }

    internal int Height { get; }

    internal int FrameRate { get; }

    public string Id { get; }

    public string Label { get; }

    public MediaStreamTrackKind Kind { get; }

    public MediaStreamTrackState ReadyState => _readyState;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_handle == IntPtr.Zero || _enabled == value)
                return;

            WebRtcRuntime.Check(MediaTrackSetEnabled(_handle, value ? 1 : 0),
                                $"set track '{Id}' enabled to {value}");
            _enabled = value;
        }
    }

    /// <summary>Not reported by the ABI; a live track is treated as unmuted.</summary>
    public bool Muted => _readyState == MediaStreamTrackState.Ended;

    /// <summary>The ABI exposes no content hint, so this is remembered rather than applied.</summary>
    public string ContentHint { get; set; } = string.Empty;

    public bool Isolated => false;

    public event EventHandler OnEnded;

    // The ABI raises no mute signal; a track's own Enabled is the only thing that changes here,
    // and that is a local decision the caller already knows about.
#pragma warning disable CS0067
    public event EventHandler OnMute;
    public event EventHandler OnUnmute;
#pragma warning restore CS0067

    /// <summary>
    /// Attaches a frame handler, starting the native sink if this is the first one. Frames are
    /// delivered as tightly packed BGRA8 on a WebRTC thread.
    /// </summary>
    internal IDisposable SubscribeToFrames(Action<byte[], int, int> onBgraFrame)
    {
        if (Kind != MediaStreamTrackKind.Video)
            throw new ArgumentException("Frames can only be read from a video track.");

        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

            Volatile.Write(ref _subscribers, [.. _subscribers, onBgraFrame]);

            if (!_sinkAttached)
            {
                _self = GCHandle.Alloc(this);
                unsafe
                {
                    WebRtcRuntime.Check(
                        VideoTrackAddSink(_handle, &OnFrame, GCHandle.ToIntPtr(_self)),
                        $"attach a frame sink to track '{Id}'");
                }
                _sinkAttached = true;
            }
        }

        return new Subscription(() => Unsubscribe(onBgraFrame));
    }

    private void Unsubscribe(Action<byte[], int, int> onBgraFrame)
    {
        lock (_gate)
        {
            var remaining = Without(_subscribers, onBgraFrame);
            if (remaining is null)
                return;
            Volatile.Write(ref _subscribers, remaining);

            if (remaining.Length == 0)
                DetachSink();
        }
    }

    /// <summary>The array without the first occurrence of <paramref name="item"/>, or null when it is not there.</summary>
    private static Action<byte[], int, int>[] Without(Action<byte[], int, int>[] items, Action<byte[], int, int> item)
    {
        var index = Array.IndexOf(items, item);
        if (index < 0)
            return null;
        if (items.Length == 1)
            return NoSubscribers;

        var remaining = new Action<byte[], int, int>[items.Length - 1];
        Array.Copy(items, 0, remaining, 0, index);
        Array.Copy(items, index + 1, remaining, index, items.Length - index - 1);
        return remaining;
    }

    /// <summary>
    /// Caller holds <see cref="_gate"/>. The native remove returns only once the frame thread is
    /// out of <see cref="OnFrame"/>, which is what makes freeing the handle afterwards safe -- and
    /// why <see cref="OnFrame"/> must never wait for the gate.
    /// </summary>
    private void DetachSink()
    {
        if (!_sinkAttached)
            return;

        if (_handle != IntPtr.Zero)
            VideoTrackRemoveSink(_handle);

        _sinkAttached = false;
        _self.Free();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnFrame(IntPtr userData, Frame* frame)
    {
        var track = (MediaStreamTrack)GCHandle.FromIntPtr(userData).Target;
        if (track is null || frame->Y == IntPtr.Zero || frame->Width <= 0 || frame->Height <= 0)
            return;

        // No lock here, ever: an unsubscribe on another thread holds the gate while the native
        // remove-sink waits for this very thread to finish. The current array is enough; one
        // that changed a moment ago delivers one more frame to a handler that just left, which
        // the middleware tolerates, while a lock would deadlock the application.
        var subscribers = Volatile.Read(ref track._subscribers);
        if (subscribers.Length == 0)
            return;

        // A fresh buffer per frame: the middleware hands it to the UI thread and reads it after
        // this call has returned, so a shared buffer would tear.
        var bgra = new byte[FrameConverter.BgraLength(frame->Width, frame->Height)];
        FrameConverter.ToBgra(*frame, bgra);

        // The size as it should be *seen*, which is not the size of the buffer when the sender
        // turned the camera: a phone in portrait sends 640x480 tagged for a quarter turn and the
        // viewer should get 480x640. The converter has already applied the turn, so subscribers
        // are handed an upright frame and never have to know.
        var width = FrameConverter.DisplayWidth(*frame);
        var height = FrameConverter.DisplayHeight(*frame);

        foreach (var subscriber in subscribers)
        {
            try
            {
                subscriber(bgra, width, height);
            }
            catch (Exception ex)
            {
                // Letting this escape would unwind into native code across the ABI.
                System.Diagnostics.Debug.WriteLine($"######## Video frame handler failed: {ex}");
            }
        }
    }

    public Task ApplyConstraints(MediaTrackConstraints constraints) =>
        throw new NotSupportedException(
            "Re-applying constraints to a live track is not supported by the Windows binding; " +
            "request the track again with the constraints you want.");

    public IMediaStreamTrack Clone() =>
        throw new NotSupportedException("Cloning a track is not supported by the Windows binding.");

    public MediaTrackCapabilities GetCapabilities() => Kind == MediaStreamTrackKind.Video
        ? new MediaTrackCapabilities
        {
            DeviceId = DeviceId,
            GroupId = DeviceId,
            Width = new ULongRange { Min = 0, Max = (ulong)Width },
            Height = new ULongRange { Min = 0, Max = (ulong)Height },
            FrameRate = new DoubleRange { Min = 0, Max = FrameRate }
        }
        : new MediaTrackCapabilities { DeviceId = DeviceId, GroupId = DeviceId };

    public MediaTrackConstraints GetConstraints() => new()
    {
        DeviceId = DeviceId is null ? null : new ConstrainDOMString { Value = DeviceId }
    };

    public MediaTrackSettings GetSettings() => new()
    {
        DeviceId = DeviceId,
        GroupId = DeviceId,
        Width = Width,
        Height = Height,
        FrameRate = FrameRate,
        AspectRatio = Height == 0 ? 0 : (double)Width / Height
    };

    /// <summary>
    /// Ends the track. The ABI has no stop, so capture is silenced by disabling the track; the
    /// underlying device is released when the handle is.
    /// </summary>
    public void Stop()
    {
        if (_readyState == MediaStreamTrackState.Ended)
            return;

        // Before the event, so that a listener which reacts by opening another track cannot be
        // beaten to it by a watcher still holding this one.
        CaptureDeviceWatcher.Forget(this);

        Enabled = false;
        _readyState = MediaStreamTrackState.Ended;
        OnEnded?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        IntPtr handle;

        CaptureDeviceWatcher.Forget(this);

        lock (_gate)
        {
            if (_handle == IntPtr.Zero)
                return;

            Volatile.Write(ref _subscribers, NoSubscribers);
            DetachSink();

            handle = _handle;
            _handle = IntPtr.Zero;
        }

        _readyState = MediaStreamTrackState.Ended;
        MediaTrackRelease(handle);
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        private Action _unsubscribe = unsubscribe;

        public void Dispose()
        {
            var unsubscribe = _unsubscribe;
            _unsubscribe = null;
            unsubscribe?.Invoke();
        }
    }
}
