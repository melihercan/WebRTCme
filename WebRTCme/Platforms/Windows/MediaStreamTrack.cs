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
/// The ABI allows a single frame sink per track, so this owns that one sink and fans frames out
/// to however many subscribers the app attaches -- a local track feeding both a self-preview and
/// something else would otherwise fail with INVALID_STATE on the second attach.
/// </remarks>
internal sealed class MediaStreamTrack : IMediaStreamTrack
{
    private readonly List<Action<byte[], int, int>> _subscribers = [];
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

            _subscribers.Add(onBgraFrame);

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
            _subscribers.Remove(onBgraFrame);

            if (_subscribers.Count == 0)
                DetachSink();
        }
    }

    /// <summary>Caller holds <see cref="_gate"/>.</summary>
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

        // Snapshot the handlers rather than converting under the lock: this is the capture
        // thread, and holding it here would stall the pipeline behind an unsubscribe.
        Action<byte[], int, int>[] subscribers;
        lock (track._gate)
        {
            if (track._subscribers.Count == 0)
                return;
            subscribers = [.. track._subscribers];
        }

        // A fresh buffer per frame: the middleware hands it to the UI thread and reads it after
        // this call has returned, so a shared buffer would tear.
        var bgra = new byte[FrameConverter.BgraLength(frame->Width, frame->Height)];
        FrameConverter.ToBgra(*frame, bgra);

        foreach (var subscriber in subscribers)
        {
            try
            {
                subscriber(bgra, frame->Width, frame->Height);
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

            _subscribers.Clear();
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
