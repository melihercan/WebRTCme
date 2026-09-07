using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// What <c>add_track</c> hands back: the handle through which a track can be swapped or removed
/// after the fact.
/// </summary>
/// <remarks>
/// <see cref="Track"/> is what this sender is currently sending, updated by
/// <see cref="ReplaceTrack"/> — so <c>GetSenders().First(s =&gt; s.Track.Kind == ...)</c> keeps
/// finding the right sender after a swap, which is how the signalling layer uses it.
/// </remarks>
internal sealed class RTCRtpSender : IRTCRtpSender
{
    private IntPtr _handle;

    internal RTCRtpSender(IntPtr handle, IMediaStreamTrack track)
    {
        _handle = handle;
        Track = track;
    }

    internal IntPtr Handle => _handle;

    public IMediaStreamTrack Track { get; private set; }

    /// <summary>
    /// Swaps the track without renegotiating — the whole point of replaceTrack over
    /// remove-then-add. A null track stops the sender while leaving the transport in place, which
    /// is how muting is done at the sender rather than the source.
    /// </summary>
    public Task ReplaceTrack(IMediaStreamTrack newTrack = null)
    {
        ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

        var handle = IntPtr.Zero;
        if (newTrack is not null)
        {
            if (newTrack is not MediaStreamTrack windowsTrack)
                throw new ArgumentException(
                    $"Track must come from the Windows binding, got {newTrack.GetType().FullName}.",
                    nameof(newTrack));

            if (newTrack.Kind != Track?.Kind)
                throw new ArgumentException(
                    $"A {Track?.Kind} sender cannot take a {newTrack.Kind} track.",
                    nameof(newTrack));

            handle = windowsTrack.Handle;
        }

        WebRtcRuntime.Check(RtpSenderReplaceTrack(_handle, handle),
                            "replace the track on the sender");

        Track = newTrack;
        return Task.CompletedTask;
    }

    /// <summary>Called after the peer connection has removed this sender.</summary>
    internal void Detach() => Track = null;

    public IRTCDTMFSender Dtmf =>
        throw new NotSupportedException("DTMF is not supported by the Windows binding.");

    public IRTCDtlsTransport Transport =>
        throw new NotSupportedException("Transport details are not exposed by the Windows binding.");

    public RTCRtpCapabilities GetCapabilities(string kind) =>
        throw new NotSupportedException("Sender capabilities are not reported by the Windows binding.");

    public RTCRtpSendParameters GetParameters() =>
        throw new NotSupportedException("Send parameters are not exposed by the Windows binding.");

    public Task<IRTCStatsReport> GetStats() =>
        throw new NotSupportedException("Stats are not exposed by the Windows binding.");

    public Task SetParameters(RTCRtpSendParameters parameters) =>
        throw new NotSupportedException(
            "Send parameters are not configurable through the Windows binding.");

    public void SetStreams(IMediaStream[] mediaStreams) =>
        throw new NotSupportedException(
            "Reassigning a sender's streams is not supported by the Windows binding.");

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle != IntPtr.Zero)
            RtpSenderRelease(handle);
    }
}
