namespace WebRTCme.Windows;

internal sealed class Window : IWindow
{
    public INavigator Navigator() => WebRTCme.Windows.Navigator.Create();

    public IMediaStream MediaStream() => new MediaStream();

    public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration = null) =>
        new RTCPeerConnection(configuration);

    public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null) =>
        throw new NotSupportedException(
            "Recording is not supported by the Windows binding: the interop ABI has no encoder " +
            "surface for it.");

    public void Dispose() { }
}
