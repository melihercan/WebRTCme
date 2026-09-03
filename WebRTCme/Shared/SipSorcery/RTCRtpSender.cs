using System;
using System.Threading.Tasks;

namespace WebRTCme.Shared.SipSorcery
{
    /// <summary>
    /// SIPSorcery has no sender object - a local track is attached directly to the peer
    /// connection - so this exists mainly to let callers find the track they added via
    /// GetSenders().
    /// </summary>
    internal class RTCRtpSender : IRTCRtpSender
    {
        public RTCRtpSender(IMediaStreamTrack track) => Track = track;

        public IMediaStreamTrack Track { get; }

        public IRTCDTMFSender Dtmf =>
            throw new NotSupportedException("DTMF is not supported by the SipSorcery binding.");

        public IRTCDtlsTransport Transport =>
            throw new NotSupportedException(
                "Transport details are not exposed by the SipSorcery binding.");

        public RTCRtpCapabilities GetCapabilities(string kind) =>
            throw new NotSupportedException(
                "Sender capabilities are not reported by the SipSorcery binding.");

        public RTCRtpSendParameters GetParameters() =>
            throw new NotSupportedException(
                "Send parameters are not exposed by the SipSorcery binding.");

        public Task<IRTCStatsReport> GetStats() =>
            throw new NotSupportedException("Stats are not supported by the SipSorcery binding.");

        public Task ReplaceTrack(IMediaStreamTrack newTrack = null) =>
            throw new NotSupportedException(
                "Replacing a track on a live sender is not supported by the SipSorcery binding.");

        public Task SetParameters(RTCRtpSendParameters parameters) =>
            throw new NotSupportedException(
                "Send parameters are not configurable through the SipSorcery binding.");

        public void SetStreams(IMediaStream[] mediaStreams) =>
            throw new NotSupportedException(
                "SIPSorcery does not group tracks into streams.");

        public void Dispose() { }
    }
}
