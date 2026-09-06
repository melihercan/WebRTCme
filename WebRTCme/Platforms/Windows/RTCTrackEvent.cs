namespace WebRTCme.Windows;

internal sealed class RTCTrackEvent(IMediaStreamTrack track, IMediaStream[] streams = null)
    : IRTCTrackEvent
{
    public IMediaStreamTrack Track { get; } = track;

    public IMediaStream[] Streams { get; } = streams ?? [];

    /// <summary>The shim delivers the track itself; there is no receiver object behind it.</summary>
    public IRTCRtpReceiver Receiver => null;

    /// <summary>The shim exposes no transceivers.</summary>
    public IRTCRtpTransceiver Transceiver => null;

    public void Dispose() { }
}
