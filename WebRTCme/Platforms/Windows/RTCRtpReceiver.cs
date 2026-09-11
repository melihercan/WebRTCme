using System.Runtime.InteropServices;
using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// The receiving half of a transceiver: the remote track arriving on one m-section.
/// </summary>
/// <remarks>
/// The track exists as soon as the transceiver does, before any media has arrived, which is what
/// lets a client wire up rendering during negotiation rather than waiting for the first frame.
/// </remarks>
internal sealed class RTCRtpReceiver : IRTCRtpReceiver
{
    private readonly RTCPeerConnection _peerConnection;
    private IntPtr _handle;
    private IMediaStreamTrack _track;

    internal RTCRtpReceiver(RTCPeerConnection peerConnection, IntPtr handle)
    {
        _peerConnection = peerConnection;
        _handle = handle;
    }

    internal IntPtr Handle => _handle;

    /// <summary>
    /// The remote track, fetched once and remembered.
    /// </summary>
    /// <remarks>
    /// Remembered because each native call hands back a new handle onto the same track, and
    /// returning a different wrapper every time would break any caller comparing what it got last
    /// time with what it got now — which is exactly how an incoming stream is matched to the
    /// section carrying it.
    /// </remarks>
    public IMediaStreamTrack Track
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

            if (_track is not null)
                return _track;

            var status = RtpReceiverGetTrack(_handle, out var track);

            // A transceiver that has been stopped has no track. That is a state, not a failure,
            // and a caller asking during teardown should get null rather than an exception.
            if (status == ErrNotFound)
                return null;

            WebRtcRuntime.Check(status, "get the receiver's track");

            // Both the kind and the id have to be asked for: a remote track arrived through
            // negotiation, so unlike one this process created, nothing here already knows them.
            WebRtcRuntime.Check(MediaTrackGetKind(track, out var kind),
                                "get the receiver track's kind");

            string id = null;
            if (MediaTrackGetId(track, out var native) == Ok)
                id = WebRtcRuntime.TakeString(native);

            _track = new MediaStreamTrack(
                track,
                kind == MediaKindVideo ? MediaStreamTrackKind.Video : MediaStreamTrackKind.Audio,
                id ?? Guid.NewGuid().ToString(),
                label: string.Empty,
                isRemote: true);
            return _track;
        }
    }

    public Task<IRTCStatsReport> GetStats()
    {
        ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);
        return _peerConnection.GetReceiverStats(_handle);
    }

    public IRTCDtlsTransport Transport =>
        throw new NotSupportedException("Transport details are not exposed by the Windows binding.");

    public double? JitterBufferTarget
    {
        get => throw new NotSupportedException(
            "The jitter buffer target is not exposed by the Windows binding.");
        set => throw new NotSupportedException(
            "The jitter buffer target is not configurable through the Windows binding.");
    }

    public RTCRtpContributingSource[] GetContributingSources() =>
        throw new NotSupportedException(
            "Contributing sources are not reported by the Windows binding.");

    public RTCRtpReceiveParameters GetParameters() =>
        throw new NotSupportedException(
            "Receive parameters are not exposed by the Windows binding.");

    public RTCRtpSynchronizationSource[] GetSynchronizationSources() =>
        throw new NotSupportedException(
            "Synchronization sources are not reported by the Windows binding.");

    public RTCRtpCapabilities GetCapabilities(string kind) =>
        throw new NotSupportedException(
            "Receiver capabilities are not reported by the Windows binding.");

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle != IntPtr.Zero)
            RtpReceiverRelease(handle);
    }
}
