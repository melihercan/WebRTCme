namespace WebRTCme.Windows;

internal sealed class RTCPeerConnectionIceEvent(RTCIceCandidateInit candidate)
    : IRTCPeerConnectionIceEvent
{
    public IRTCIceCandidate Candidate { get; } = new RTCIceCandidate(candidate);

    public void Dispose() { }
}
