using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// One m-section of the negotiation, pairing a sender with a receiver.
/// </summary>
/// <remarks>
/// The peer connection owns the underlying transceiver; this is a view onto it. Several wrappers
/// can therefore refer to the same transceiver, because <c>GetTransceivers</c> builds a fresh set
/// every time it is called. Disposing one drops a reference and nothing more — it does not stop
/// the transceiver, and it does not disturb any other wrapper onto it.
/// </remarks>
internal sealed class RTCRtpTransceiver : IRTCRtpTransceiver
{
    private readonly RTCPeerConnection _peerConnection;
    private IntPtr _handle;
    private RTCRtpSender _sender;
    private RTCRtpReceiver _receiver;

    internal RTCRtpTransceiver(RTCPeerConnection peerConnection, IntPtr handle)
    {
        _peerConnection = peerConnection;
        _handle = handle;
    }

    internal IntPtr Handle => _handle;

    /// <summary>
    /// The m-section identifier, or null until the local description naming it has been applied.
    /// </summary>
    /// <remarks>
    /// Null rather than an exception, because reading it before <c>SetLocalDescription</c> is the
    /// ordinary mistake here and the W3C property is nullable for exactly that reason. Not cached:
    /// it goes from null to a value as negotiation proceeds, so a remembered null would stay null.
    /// </remarks>
    public string Mid
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

            var status = RtpTransceiverGetMid(_handle, out var native);
            if (status == ErrNotFound)
                return null;

            WebRtcRuntime.Check(status, "get the transceiver's mid");
            return WebRtcRuntime.TakeString(native);
        }
    }

    /// <summary>
    /// The sender, fetched once and remembered so the same wrapper is handed back every time.
    /// </summary>
    /// <remarks>
    /// Remembering matters more than saving a call: <see cref="RTCRtpSender.ReplaceTrack"/> keeps
    /// the current track on the wrapper, and a fresh wrapper each time would forget it.
    /// </remarks>
    public IRTCRtpSender Sender
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

            if (_sender is not null)
                return _sender;

            WebRtcRuntime.Check(RtpTransceiverGetSender(_handle, out var sender),
                                "get the transceiver's sender");

            // The track is not known here. A send transceiver was created with one, and the
            // caller holds it; a receive-only transceiver has none.
            _sender = new RTCRtpSender(_peerConnection, sender, track: null);
            return _sender;
        }
    }

    public IRTCRtpReceiver Receiver
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

            if (_receiver is not null)
                return _receiver;

            WebRtcRuntime.Check(RtpTransceiverGetReceiver(_handle, out var receiver),
                                "get the transceiver's receiver");

            _receiver = new RTCRtpReceiver(_peerConnection, receiver);
            return _receiver;
        }
    }

    public RTCRtpTransceiverDirection Direction
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);
            WebRtcRuntime.Check(RtpTransceiverGetDirection(_handle, out var direction),
                                "get the transceiver's direction");
            return ToDirection(direction);
        }
        set
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);
            WebRtcRuntime.Check(RtpTransceiverSetDirection(_handle, FromDirection(value)),
                                "set the transceiver's direction");
        }
    }

    /// <summary>
    /// What negotiation settled on, which is <see cref="RTCRtpTransceiverDirection.Inactive"/>
    /// until an answer has been exchanged.
    /// </summary>
    /// <remarks>
    /// The W3C property is nullable and this one is not, so "not yet negotiated" has to land
    /// somewhere. Inactive is the honest choice: nothing is flowing in either direction yet.
    /// </remarks>
    public RTCRtpTransceiverDirection CurrentDirection
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

            var status = RtpTransceiverGetCurrentDirection(_handle, out var direction);
            if (status == ErrNotFound)
                return RTCRtpTransceiverDirection.Inactive;

            WebRtcRuntime.Check(status, "get the transceiver's current direction");
            return ToDirection(direction);
        }
    }

    public void Stop()
    {
        ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);
        WebRtcRuntime.Check(RtpTransceiverStop(_handle), "stop the transceiver");
    }

    // Codec preferences would need the codec capability tables across the ABI, which nothing
    // exports yet. mediasoup does its codec selection in the SDP instead, so this is not on the
    // path that made transceivers necessary.
    public void SetCodecPreferences(RTCRtpCodec[] codecs) =>
        throw new NotSupportedException(
            "Codec preferences are not configurable through the Windows binding.");

    internal static RTCRtpTransceiverDirection ToDirection(int direction) => direction switch
    {
        TransceiverDirectionSendRecv => RTCRtpTransceiverDirection.SendRecv,
        TransceiverDirectionSendOnly => RTCRtpTransceiverDirection.SendOnly,
        TransceiverDirectionRecvOnly => RTCRtpTransceiverDirection.RecvOnly,
        TransceiverDirectionStopped => RTCRtpTransceiverDirection.Stopped,
        _ => RTCRtpTransceiverDirection.Inactive
    };

    /// <summary>
    /// Maps to the ABI. Stopped is refused rather than translated: a transceiver is stopped by
    /// <see cref="Stop"/>, and WebRTC rejects the value outright.
    /// </summary>
    internal static int FromDirection(RTCRtpTransceiverDirection direction) => direction switch
    {
        RTCRtpTransceiverDirection.SendRecv => TransceiverDirectionSendRecv,
        RTCRtpTransceiverDirection.SendOnly => TransceiverDirectionSendOnly,
        RTCRtpTransceiverDirection.RecvOnly => TransceiverDirectionRecvOnly,
        RTCRtpTransceiverDirection.Inactive => TransceiverDirectionInactive,
        _ => throw new ArgumentOutOfRangeException(
            nameof(direction), direction,
            "A transceiver cannot be set to Stopped; call Stop() instead.")
    };

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle == IntPtr.Zero)
            return;

        _sender?.Dispose();
        _receiver?.Dispose();
        RtpTransceiverRelease(handle);
    }
}
