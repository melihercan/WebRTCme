using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
{
    internal class RTCRtpSender : NativeBase<Webrtc.IRTCRtpSender>, IRTCRtpSender
    {
        public RTCRtpSender(Webrtc.IRTCRtpSender nativeRtpSender) : base(nativeRtpSender)
        { }

        public IRTCDTMFSender Dtmf => new RTCDTMFSender(NativeObject.DtmfSender);

        public IMediaStreamTrack Track => new MediaStreamTrack(NativeObject.Track);

        public IRTCDtlsTransport Transport => throw new NotImplementedException();

        public RTCRtpCapabilities GetCapabilities(string kind)
        {
            throw new NotImplementedException();
        }

        public RTCRtpSendParameters GetParameters() => NativeObject.Parameters
            .FromNativeToSend();

        public Task<IRTCStatsReport> GetStats()
        {
            // The Objective-C category binds statisticsForSender:/statisticsForReceiver: to the
            // generated RTCRtpSender/RTCRtpReceiver classes, but senders and receivers reach us as
            // protocol wrappers, so there is nothing valid to pass. Reaching them needs the binding
            // definition changed to take the protocol interface.
            throw new NotSupportedException(
                "Per-sender stats are not reachable through the current iOS binding; "
                + "use RTCPeerConnection.GetStats() and select the sender entries from the report.");
        }

        public Task ReplaceTrack(IMediaStreamTrack newTrack = null)
        {
            throw new NotImplementedException();
        }

        public Task SetParameters(RTCRtpSendParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void SetStreams(IMediaStream[] mediaStreams)
        {
            throw new NotImplementedException();
        }
    }
}
