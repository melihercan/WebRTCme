using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// A peer connection over the interop shim.
/// </summary>
/// <remarks>
/// <para>
/// Every callback the shim raises arrives on WebRTC's signalling thread, which is shared by every
/// connection in the process, and calls back into the ABI block on it. Raising the .NET events
/// directly from there would put application code on that thread, where one synchronous call back
/// into the binding -- adding a candidate, creating an answer -- deadlocks the thread against
/// itself. So callbacks copy what they need and queue it, and a single reader raises the events in
/// order on the thread pool. Task completions use <see cref="TaskCreationOptions.RunContinuationsAsynchronously"/>
/// for the same reason: an awaiting continuation must not resume on the signalling thread.
/// </para>
/// <para>
/// The shim covers offer/answer, ICE and tracks. Data channels, transceivers, receivers and stats
/// have no ABI functions behind them and throw; they need to be added to the shim in the
/// WebRTCnative repository before they can be supported here.
/// </para>
/// </remarks>
internal sealed class RTCPeerConnection : IRTCPeerConnection
{
    private readonly EventDispatcher _events = new();
    private readonly List<IRTCRtpSender> _senders = [];
    private readonly RTCConfiguration _configuration;
    private readonly GCHandle _self;

    private IntPtr _handle;
    private RTCPeerConnectionState _connectionState = RTCPeerConnectionState.New;
    private RTCSignalingState _signalingState = RTCSignalingState.Stable;
    private RTCIceGatheringState _iceGatheringState = RTCIceGatheringState.New;

    internal RTCPeerConnection(RTCConfiguration configuration)
    {
        _configuration = configuration ?? new RTCConfiguration();
        _self = GCHandle.Alloc(this);

        unsafe
        {
            var observer = new PeerConnectionObserver
            {
                OnIceCandidate = &RaiseIceCandidate,
                OnConnectionState = &RaiseConnectionState,
                OnSignalingState = &RaiseSignalingState,
                OnTrack = &RaiseTrack,
                OnRenegotiationNeeded = &RaiseRenegotiationNeeded,
                OnDataChannel = &RaiseDataChannel
            };

            using var servers = new NativeIceServers(_configuration.IceServers);

            var status = PeerConnectionCreate(WebRtcRuntime.Factory, servers.Configuration,
                                              observer, GCHandle.ToIntPtr(_self), out _handle);
            if (status != Ok)
            {
                _self.Free();
                WebRtcRuntime.Check(status, "create a peer connection");
            }
        }
    }

    public RTCPeerConnectionState ConnectionState => _connectionState;

    public RTCSignalingState SignalingState => _signalingState;

    public RTCIceGatheringState IceGatheringState => _iceGatheringState;

    /// <summary>
    /// Derived from <see cref="ConnectionState"/>. The shim reports one aggregate state rather
    /// than the ICE transport's own, and for a connection with a single transport -- which is
    /// every connection here, since the shim bundles -- the two track each other.
    /// </summary>
    public RTCIceConnectionState IceConnectionState => _connectionState switch
    {
        RTCPeerConnectionState.New => RTCIceConnectionState.New,
        RTCPeerConnectionState.Connecting => RTCIceConnectionState.Checking,
        RTCPeerConnectionState.Connected => RTCIceConnectionState.Connected,
        RTCPeerConnectionState.Disconnected => RTCIceConnectionState.Disconnected,
        RTCPeerConnectionState.Failed => RTCIceConnectionState.Failed,
        _ => RTCIceConnectionState.Closed
    };

    public RTCSessionDescriptionInit LocalDescription { get; private set; }

    public RTCSessionDescriptionInit RemoteDescription { get; private set; }

    public RTCSessionDescriptionInit CurrentLocalDescription => LocalDescription;

    public RTCSessionDescriptionInit CurrentRemoteDescription => RemoteDescription;

    /// <summary>The shim applies a description immediately, so nothing is ever pending.</summary>
    public RTCSessionDescriptionInit PendingLocalDescription => null;

    public RTCSessionDescriptionInit PendingRemoteDescription => null;

    public bool CanTrickleIceCandidates => true;

    public event EventHandler OnConnectionStateChanged;
    public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
    public event EventHandler<IRTCPeerConnectionIceErrorEvent> OnIceCandidateError;
    public event EventHandler OnIceConnectionStateChange;
    public event EventHandler OnIceGatheringStateChange;
    public event EventHandler OnNegotiationNeeded;
    public event EventHandler OnSignalingStateChange;
    public event EventHandler<IRTCTrackEvent> OnTrack;

    public event EventHandler<IRTCDataChannelEvent> OnDataChannel;

    // ---- negotiation --------------------------------------------------------

    public Task<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions options = null) =>
        CreateDescription(offer: true);

    public Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options = null) =>
        CreateDescription(offer: false);

    private unsafe Task<RTCSessionDescriptionInit> CreateDescription(bool offer)
    {
        ThrowIfClosed();

        var completion = new TaskCompletionSource<RTCSessionDescriptionInit>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var context = GCHandle.Alloc(completion);

        var status = offer
            ? PeerConnectionCreateOffer(_handle, &SdpSucceeded, &SdpFailed, GCHandle.ToIntPtr(context))
            : PeerConnectionCreateAnswer(_handle, &SdpSucceeded, &SdpFailed, GCHandle.ToIntPtr(context));

        if (status != Ok)
        {
            context.Free();
            WebRtcRuntime.Check(status, offer ? "create an offer" : "create an answer");
        }

        return completion.Task;
    }

    /// <summary>
    /// The shim has no argument-less form, so this follows the algorithm the spec gives for it:
    /// answer when a remote offer is outstanding, otherwise offer.
    /// </summary>
    public async Task SetLocalDescription()
    {
        var description = _signalingState is RTCSignalingState.HaveRemoteOffer
                                          or RTCSignalingState.HaveLocalPranswer
            ? await CreateAnswer().ConfigureAwait(false)
            : await CreateOffer().ConfigureAwait(false);

        await SetLocalDescription(description).ConfigureAwait(false);
    }

    public Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription)
    {
        ArgumentNullException.ThrowIfNull(sessionDescription);

        return SetDescription(sessionDescription, local: true).ContinueWith(task =>
        {
            task.GetAwaiter().GetResult();
            LocalDescription = sessionDescription;

            // Gathering starts when the local description is applied. The shim raises no
            // completion signal, so the state stops at Gathering rather than reaching Complete.
            UpdateIceGatheringState(RTCIceGatheringState.Gathering);
        }, TaskScheduler.Default);
    }

    public Task SetRemoteDescription(RTCSessionDescriptionInit sessionDescription)
    {
        ArgumentNullException.ThrowIfNull(sessionDescription);

        return SetDescription(sessionDescription, local: false).ContinueWith(task =>
        {
            task.GetAwaiter().GetResult();
            RemoteDescription = sessionDescription;
        }, TaskScheduler.Default);
    }

    private unsafe Task SetDescription(RTCSessionDescriptionInit description, bool local)
    {
        ThrowIfClosed();

        var type = SdpTypeName(description.Type);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var context = GCHandle.Alloc(completion);

        var status = local
            ? PeerConnectionSetLocalDescription(_handle, type, description.Sdp,
                                                &SetSucceeded, &SetFailed, GCHandle.ToIntPtr(context))
            : PeerConnectionSetRemoteDescription(_handle, type, description.Sdp,
                                                  &SetSucceeded, &SetFailed, GCHandle.ToIntPtr(context));

        if (status != Ok)
        {
            context.Free();
            WebRtcRuntime.Check(status, $"set the {(local ? "local" : "remote")} description");
        }

        return completion.Task;
    }

    public Task AddIceCandidate(RTCIceCandidateInit candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ThrowIfClosed();

        WebRtcRuntime.Check(
            PeerConnectionAddIceCandidate(_handle, candidate.SdpMid ?? string.Empty,
                                          candidate.SdpMLineIndex ?? 0, candidate.Candidate),
            "add a remote ICE candidate");

        return Task.CompletedTask;
    }

    // ---- tracks -------------------------------------------------------------

    public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream)
    {
        ArgumentNullException.ThrowIfNull(track);
        ThrowIfClosed();

        if (track is not MediaStreamTrack windowsTrack)
            throw new ArgumentException(
                $"Track must come from the Windows binding, got {track.GetType().FullName}.",
                nameof(track));

        WebRtcRuntime.Check(
            PeerConnectionAddTrack(_handle, windowsTrack.Handle, stream?.Id ?? string.Empty,
                                   out var handle),
            $"add track '{track.Id}' to the peer connection");

        var sender = new RTCRtpSender(this, handle, track);
        lock (_senders)
            _senders.Add(sender);
        return sender;
    }

    public void RemoveTrack(IRTCRtpSender sender)
    {
        if (sender is null)
            return;

        if (sender is not RTCRtpSender windowsSender)
            throw new ArgumentException(
                $"Sender must come from the Windows binding, got {sender.GetType().FullName}.",
                nameof(sender));

        lock (_senders)
            _senders.Remove(sender);

        if (_handle == IntPtr.Zero || windowsSender.Handle == IntPtr.Zero)
            return;

        // Idempotent, per W3C: removing a sender whose track is already gone is a quiet no-op.
        WebRtcRuntime.Check(PeerConnectionRemoveTrack(_handle, windowsSender.Handle),
                            "remove the track from the peer connection");
        windowsSender.Detach();
    }

    public IRTCRtpSender[] GetSenders()
    {
        lock (_senders)
            return [.. _senders];
    }

    // ---- data channels ------------------------------------------------------

    /// <summary>
    /// Opens a data channel. Called before the offer it appears as an m=application section;
    /// called afterwards it raises <see cref="OnNegotiationNeeded"/>, as in the W3C API.
    /// </summary>
    public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options = null)
    {
        ArgumentNullException.ThrowIfNull(label);
        ThrowIfClosed();

        // The ABI takes UTF-8 and -1 for the unset optionals.
        var protocol = options?.Protocol is null
            ? IntPtr.Zero
            : Marshal.StringToCoTaskMemUTF8(options.Protocol);

        try
        {
            var init = new DataChannelInit
            {
                Protocol = protocol,
                Ordered = (options?.Ordered ?? true) ? 1 : 0,
                MaxPacketLifeTime = options?.MaxPacketLifeTime ?? -1,
                MaxRetransmits = options?.MaxRetransmits ?? -1,
                Negotiated = (options?.Negotiated ?? false) ? 1 : 0,
                Id = options?.Id ?? -1
            };

            WebRtcRuntime.Check(
                PeerConnectionCreateDataChannel(_handle, label, init, out var channel),
                $"create the data channel '{label}'");

            return new RTCDataChannel(channel, options);
        }
        finally
        {
            // Safe to free straight away: the shim copies the configuration into WebRTC's own
            // during the call and keeps none of these pointers.
            if (protocol != IntPtr.Zero)
                Marshal.FreeCoTaskMem(protocol);
        }
    }

    // ---- lifetime -----------------------------------------------------------

    public void Close()
    {
        if (_handle == IntPtr.Zero)
            return;

        WebRtcRuntime.Check(PeerConnectionClose(_handle), "close the peer connection");
        UpdateConnectionState(RTCPeerConnectionState.Closed);
        UpdateSignalingState(RTCSignalingState.Closed);
    }

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle == IntPtr.Zero)
            return;

        PeerConnectionClose(handle);
        _events.Dispose();

        lock (_senders)
        {
            foreach (var sender in _senders)
                sender.Dispose();
            _senders.Clear();
        }

        // Releasing revokes the observer, so no callback can arrive after this point and the
        // GCHandle behind user_data is safe to free.
        PeerConnectionRelease(handle);
        _self.Free();
    }

    private void ThrowIfClosed() => ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

    // ---- observer callbacks -------------------------------------------------
    // Static, and doing as little as possible: copy the borrowed strings, queue, return.

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void RaiseIceCandidate(IntPtr userData, IntPtr mid, int mlineIndex, IntPtr sdp)
    {
        var self = FromUserData(userData);
        if (self is null)
            return;

        var candidate = new RTCIceCandidateInit
        {
            SdpMid = Marshal.PtrToStringUTF8(mid),
            SdpMLineIndex = (ushort)mlineIndex,
            Candidate = Marshal.PtrToStringUTF8(sdp)
        };

        self.Post(() => self.OnIceCandidate?.Invoke(self, new RTCPeerConnectionIceEvent(candidate)));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void RaiseConnectionState(IntPtr userData, int state) =>
        FromUserData(userData)?.UpdateConnectionState(ToConnectionState(state));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void RaiseSignalingState(IntPtr userData, int state) =>
        FromUserData(userData)?.UpdateSignalingState(ToSignalingState(state));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void RaiseTrack(IntPtr userData, IntPtr track, int kind, IntPtr streamId)
    {
        var self = FromUserData(userData);
        if (self is null)
        {
            // Rule 1: the handle is ours even if we have nowhere to put it.
            MediaTrackRelease(track);
            return;
        }

        var trackKind = kind == MediaKindVideo ? MediaStreamTrackKind.Video : MediaStreamTrackKind.Audio;
        var stream = Marshal.PtrToStringUTF8(streamId);

        // The id is read on the dispatcher rather than here: reading it goes back through the
        // ABI, which must not happen on the signalling thread.
        self.Post(() =>
        {
            string id = null;
            if (MediaTrackGetId(track, out var native) == Ok)
                id = WebRtcRuntime.TakeString(native);

            var remote = new MediaStreamTrack(track, trackKind, id ?? Guid.NewGuid().ToString(),
                                              label: stream, isRemote: true);

            self.OnTrack?.Invoke(self, new RTCTrackEvent(remote,
                [new MediaStream(stream, [remote])]));
        });
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void RaiseDataChannel(IntPtr userData, IntPtr channel)
    {
        var self = FromUserData(userData);
        if (self is null)
        {
            // Rule 1: the handle is ours even if we have nowhere to put it.
            DataChannelRelease(channel);
            return;
        }

        // Wrapped here rather than on the dispatcher: the wrapper registers the observer, and
        // the channel can open before a queued action would run -- which would lose OnOpen.
        // The constructor only reads the label and registers, so it does not block signalling.
        var wrapper = new RTCDataChannel(channel);

        self.Post(() => self.OnDataChannel?.Invoke(self, new RTCDataChannelEvent(wrapper)));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void RaiseRenegotiationNeeded(IntPtr userData)
    {
        var self = FromUserData(userData);
        self?.Post(() => self.OnNegotiationNeeded?.Invoke(self, EventArgs.Empty));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void SdpSucceeded(IntPtr userData, IntPtr type, IntPtr sdp)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var completion = (TaskCompletionSource<RTCSessionDescriptionInit>)handle.Target;
        handle.Free();

        completion.TrySetResult(new RTCSessionDescriptionInit
        {
            Type = ToSdpType(Marshal.PtrToStringUTF8(type)),
            Sdp = Marshal.PtrToStringUTF8(sdp)
        });
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void SdpFailed(IntPtr userData, IntPtr error)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var completion = (TaskCompletionSource<RTCSessionDescriptionInit>)handle.Target;
        handle.Free();

        completion.TrySetException(new InvalidOperationException(
            Marshal.PtrToStringUTF8(error) ?? "Creating the session description failed."));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void StatsSucceeded(IntPtr userData, IntPtr json)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var completion = (TaskCompletionSource<IRTCStatsReport>)handle.Target;
        handle.Free();

        // Parsing on the signalling thread is against the spirit of the contract, but the JSON is
        // borrowed and dies when this returns, so it has to be read here. Copying the string only
        // to parse it elsewhere would cost the same walk twice.
        try
        {
            completion.TrySetResult(StatsJson.ToStatsReport(Marshal.PtrToStringUTF8(json)));
        }
        catch (Exception exception)
        {
            completion.TrySetException(exception);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void StatsFailed(IntPtr userData, IntPtr error)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var completion = (TaskCompletionSource<IRTCStatsReport>)handle.Target;
        handle.Free();

        completion.TrySetException(new InvalidOperationException(
            Marshal.PtrToStringUTF8(error) ?? "Collecting statistics failed."));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void SetSucceeded(IntPtr userData)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var completion = (TaskCompletionSource)handle.Target;
        handle.Free();
        completion.TrySetResult();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void SetFailed(IntPtr userData, IntPtr error)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var completion = (TaskCompletionSource)handle.Target;
        handle.Free();

        completion.TrySetException(new InvalidOperationException(
            Marshal.PtrToStringUTF8(error) ?? "Applying the session description failed."));
    }

    private static RTCPeerConnection FromUserData(IntPtr userData) =>
        userData == IntPtr.Zero ? null : GCHandle.FromIntPtr(userData).Target as RTCPeerConnection;

    // ---- event dispatch -----------------------------------------------------

    private void Post(Action action) => _events.Post(action);

    private void UpdateConnectionState(RTCPeerConnectionState state)
    {
        if (_connectionState == state)
            return;

        _connectionState = state;
        Post(() =>
        {
            OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            OnIceConnectionStateChange?.Invoke(this, EventArgs.Empty);
        });
    }

    private void UpdateSignalingState(RTCSignalingState state)
    {
        if (_signalingState == state)
            return;

        _signalingState = state;
        Post(() => OnSignalingStateChange?.Invoke(this, EventArgs.Empty));
    }

    private void UpdateIceGatheringState(RTCIceGatheringState state)
    {
        if (_iceGatheringState == state)
            return;

        _iceGatheringState = state;
        Post(() => OnIceGatheringStateChange?.Invoke(this, EventArgs.Empty));
    }

    // ---- mapping ------------------------------------------------------------
    //
    // Written out rather than cast: the ABI and the API agree on the connection states but not
    // on the signalling ones, where the ABI orders the pranswer values differently.

    private static RTCPeerConnectionState ToConnectionState(int state) => state switch
    {
        PeerConnectionStateNew => RTCPeerConnectionState.New,
        PeerConnectionStateConnecting => RTCPeerConnectionState.Connecting,
        PeerConnectionStateConnected => RTCPeerConnectionState.Connected,
        PeerConnectionStateDisconnected => RTCPeerConnectionState.Disconnected,
        PeerConnectionStateFailed => RTCPeerConnectionState.Failed,
        _ => RTCPeerConnectionState.Closed
    };

    private static RTCSignalingState ToSignalingState(int state) => state switch
    {
        SignalingStateStable => RTCSignalingState.Stable,
        SignalingStateHaveLocalOffer => RTCSignalingState.HaveLocalOffer,
        SignalingStateHaveLocalPrAnswer => RTCSignalingState.HaveLocalPranswer,
        SignalingStateHaveRemoteOffer => RTCSignalingState.HaveRemoteOffer,
        SignalingStateHaveRemotePrAnswer => RTCSignalingState.HaveRemotePranswer,
        _ => RTCSignalingState.Closed
    };

    private static string SdpTypeName(RTCSdpType type) => type switch
    {
        RTCSdpType.Offer => "offer",
        RTCSdpType.Answer => "answer",
        RTCSdpType.Pranswer => "pranswer",
        RTCSdpType.Rollback => "rollback",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown SDP type.")
    };

    private static RTCSdpType ToSdpType(string type) => type switch
    {
        "offer" => RTCSdpType.Offer,
        "answer" => RTCSdpType.Answer,
        "pranswer" => RTCSdpType.Pranswer,
        "rollback" => RTCSdpType.Rollback,
        _ => RTCSdpType.Offer
    };

    // ---- unsupported --------------------------------------------------------

    public RTCConfiguration GetConfiguration() => _configuration;

    public IRTCSctpTransport Sctp => null;

    // ---- transceivers -------------------------------------------------------
    //
    // The unified-plan view of the connection. These used to throw, which made this binding
    // unusable by anything negotiating per m-section -- mediasoup reads a mid, asks for simulcast
    // encodings, and matches an incoming stream to the section carrying it, and none of that is
    // reachable through addTrack alone.

    public IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind,
                                             RTCRtpTransceiverInit init = null) =>
        AddTransceiver(kind, track: null, init);

    public IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track,
                                             RTCRtpTransceiverInit init = null)
    {
        ArgumentNullException.ThrowIfNull(track);

        if (track is not MediaStreamTrack windowsTrack)
            throw new ArgumentException(
                $"Track must come from the Windows binding, got {track.GetType().FullName}.",
                nameof(track));

        return AddTransceiver(track.Kind, windowsTrack, init);
    }

    private unsafe IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind,
                                                     MediaStreamTrack track,
                                                     RTCRtpTransceiverInit init)
    {
        ThrowIfClosed();

        // SendRecv is the W3C default for addTransceiver, and it is what a caller that passes no
        // init gets everywhere else.
        var direction = RTCRtpTransceiver.FromDirection(
            init?.Direction ?? RTCRtpTransceiverDirection.SendRecv);

        var streams = init?.Streams ?? [];
        var encodings = init?.SendEncodings ?? [];

        // Every string is pinned for the duration of the call and freed afterwards: the native
        // side copies what it needs into WebRTC's own structures before returning, so nothing
        // here has to outlive the call.
        var allocations = new List<IntPtr>();
        try
        {
            var streamIds = stackalloc IntPtr[Math.Max(streams.Length, 1)];
            for (var i = 0; i < streams.Length; i++)
            {
                var id = Marshal.StringToCoTaskMemUTF8(streams[i].Id ?? string.Empty);
                allocations.Add(id);
                streamIds[i] = id;
            }

            var native = stackalloc RtpEncoding[Math.Max(encodings.Length, 1)];
            for (var i = 0; i < encodings.Length; i++)
                native[i] = ToNative(encodings[i], allocations);

            WebRtcRuntime.Check(
                PeerConnectionAddTransceiver(
                    _handle,
                    kind == MediaStreamTrackKind.Video ? MediaKindVideo : MediaKindAudio,
                    track?.Handle ?? IntPtr.Zero,
                    direction,
                    streams.Length == 0 ? null : streamIds,
                    streams.Length,
                    encodings.Length == 0 ? null : native,
                    encodings.Length,
                    out var handle),
                $"add a {kind} transceiver");

            return new RTCRtpTransceiver(this, handle);
        }
        finally
        {
            foreach (var allocation in allocations)
                Marshal.FreeCoTaskMem(allocation);
        }
    }

    /// <summary>
    /// Maps one encoding to the ABI, recording any string it allocates so the caller can free it.
    /// </summary>
    /// <remarks>
    /// The optional members cross as sentinels rather than as nullables, to keep the struct
    /// blittable: -1 for the integers, 0 for the scale factor. A legitimate scale factor cannot be
    /// 0 because it is used as a divisor, and a bitrate cannot be negative, so neither sentinel
    /// collides with a real value.
    /// </remarks>
    private static RtpEncoding ToNative(RTCRtpEncodingParameters encoding, List<IntPtr> allocations)
    {
        IntPtr Utf8(string value)
        {
            if (value is null)
                return IntPtr.Zero;
            var pointer = Marshal.StringToCoTaskMemUTF8(value);
            allocations.Add(pointer);
            return pointer;
        }

        return new RtpEncoding
        {
            Rid = Utf8(encoding.Rid),
            Active = encoding.Active ? 1 : 0,
            // Clamped rather than wrapped: a bitrate past int.MaxValue is 2 Gbit/s, which no
            // encoder will honour anyway, and silently negating it would disable the layer.
            MaxBitrate = encoding.MaxBitrate is null
                ? -1
                : (int)Math.Min(encoding.MaxBitrate.Value, int.MaxValue),
            MaxFramerate = encoding.MaxFramerate is null
                ? -1
                : (int)Math.Round(encoding.MaxFramerate.Value),
            ScaleResolutionDownBy = encoding.ScaleResolutionDownBy ?? 0.0,
            ScalabilityMode = IntPtr.Zero
        };
    }

    /// <summary>
    /// Every transceiver on the connection, including those the remote end created.
    /// </summary>
    /// <remarks>
    /// Counted first, then fetched, and the count is re-checked: a renegotiation landing between
    /// the two calls would otherwise overflow the buffer. The native side refuses a buffer that is
    /// too small having written nothing, so retrying is safe rather than merely likely to work.
    /// </remarks>
    public unsafe IRTCRtpTransceiver[] GetTransceivers()
    {
        ThrowIfClosed();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            WebRtcRuntime.Check(PeerConnectionGetTransceivers(_handle, null, 0, out var count),
                                "count the transceivers");
            if (count == 0)
                return [];

            var handles = new IntPtr[count];
            int status;
            fixed (IntPtr* buffer = handles)
                status = PeerConnectionGetTransceivers(_handle, buffer, count, out _);

            // The one case worth retrying: the set grew between the two calls.
            if (status == ErrInvalidArg)
                continue;

            WebRtcRuntime.Check(status, "get the transceivers");

            var transceivers = new IRTCRtpTransceiver[count];
            for (var i = 0; i < count; i++)
                transceivers[i] = new RTCRtpTransceiver(this, handles[i]);
            return transceivers;
        }

        throw new InvalidOperationException(
            "The transceiver set kept changing while it was being read.");
    }

    /// <summary>
    /// The receiving half of every transceiver, which is what the W3C getReceivers() is.
    /// </summary>
    public IRTCRtpReceiver[] GetReceivers() =>
        [.. GetTransceivers().Select(transceiver => transceiver.Receiver)];

    /// <summary>getStats() narrowed to one sender. Called by <see cref="RTCRtpSender"/>.</summary>
    internal unsafe Task<IRTCStatsReport> GetSenderStats(IntPtr sender) =>
        CollectStats((pc, success, failure, userData) =>
                         RtpSenderGetStats(pc, sender, success, failure, userData),
                     "collect sender statistics");

    /// <summary>getStats() narrowed to one receiver. Called by <see cref="RTCRtpReceiver"/>.</summary>
    internal unsafe Task<IRTCStatsReport> GetReceiverStats(IntPtr receiver) =>
        CollectStats((pc, success, failure, userData) =>
                         RtpReceiverGetStats(pc, receiver, success, failure, userData),
                     "collect receiver statistics");

    private unsafe delegate int StatsRequest(
        IntPtr pc,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onSuccess,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFailure,
        IntPtr userData);

    /// <summary>
    /// The bookkeeping shared by all three getStats entry points: allocate the completion, hand
    /// its handle across, and free it again if the call never started.
    /// </summary>
    private unsafe Task<IRTCStatsReport> CollectStats(StatsRequest request, string what)
    {
        ThrowIfClosed();

        var completion = new TaskCompletionSource<IRTCStatsReport>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var context = GCHandle.Alloc(completion);

        var status = request(_handle, &StatsSucceeded, &StatsFailed, GCHandle.ToIntPtr(context));

        // The callbacks free the handle; nothing will call them if the request was refused.
        if (status != Ok)
        {
            context.Free();
            WebRtcRuntime.Check(status, what);
        }

        return completion.Task;
    }

    /// <summary>
    /// Selecting by track is still not supported, although senders and receivers now can be:
    /// the ABI's selectors are a sender or a receiver, and a track maps to neither on its own —
    /// one track can be sent by several senders.
    /// </summary>
    public Task<IRTCStatsReport> GetStats(IMediaStreamTrack selector) =>
        selector is null
            ? GetStats()
            : throw new NotSupportedException(
                "Selecting statistics by track is not supported by the Windows binding; "
                + "select on the sender or receiver, or use GetStats() and filter the report.");

    public unsafe Task<IRTCStatsReport> GetStats() =>
        CollectStats((pc, success, failure, userData) =>
                         PeerConnectionGetStats(pc, success, failure, userData),
                     "collect statistics");

    public Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm) =>
        throw new NotSupportedException("Certificate generation is not exposed by the Windows binding.");

    public void RestartIce() =>
        throw new NotSupportedException("ICE restart is not supported by the Windows binding.");

    public void SetConfiguration(RTCConfiguration configuration) =>
        throw new NotSupportedException(
            "Reconfiguring a live peer connection is not supported by the Windows binding.");
}
