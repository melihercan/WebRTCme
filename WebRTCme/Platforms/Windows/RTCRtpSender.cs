namespace WebRTCme.Windows;

/// <summary>
/// Stands for a track that has been added to a peer connection. The shim's
/// <c>rtc_peer_connection_add_track</c> returns a status rather than a sender, so this carries
/// only what the caller put in -- enough for <c>GetSenders()</c> to find a track by kind, which
/// is what the signalling layer uses it for.
/// </summary>
internal sealed class RTCRtpSender(IMediaStreamTrack track) : IRTCRtpSender
{
    public IMediaStreamTrack Track { get; } = track;

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

    /// <summary>
    /// Not supported: replacing a live sender's track needs an ABI function the shim does not
    /// have yet. Renegotiating with a new track is the way to switch camera for now.
    /// </summary>
    public Task ReplaceTrack(IMediaStreamTrack newTrack = null) =>
        throw new NotSupportedException(
            "Replacing a track on a live sender is not supported by the Windows binding.");

    public Task SetParameters(RTCRtpSendParameters parameters) =>
        throw new NotSupportedException(
            "Send parameters are not configurable through the Windows binding.");

    public void SetStreams(IMediaStream[] mediaStreams) =>
        throw new NotSupportedException(
            "Reassigning a sender's streams is not supported by the Windows binding.");

    public void Dispose() { }
}
