namespace WebRTCme.Windows;

internal sealed class RTCDataChannelEvent(IRTCDataChannel channel) : IRTCDataChannelEvent
{
    public IRTCDataChannel Channel { get; } = channel;

    public void Dispose() { }
}
