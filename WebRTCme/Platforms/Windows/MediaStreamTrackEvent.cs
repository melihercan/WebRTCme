namespace WebRTCme.Windows;

internal sealed class MediaStreamTrackEvent(IMediaStreamTrack track) : IMediaStreamTrackEvent
{
    public IMediaStreamTrack Track { get; } = track;

    public void Dispose() { }
}
