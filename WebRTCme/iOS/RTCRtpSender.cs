using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCRtpSender : ApiBase, IRTCRtpSender
    {
        public static IRTCRtpSender Create(Webrtc.IRTCRtpSender nativeRtpSender) => new RTCRtpSender(nativeRtpSender);

        private RTCRtpSender(Webrtc.IRTCRtpSender nativeRtpSender) : base(nativeRtpSender)
        {
        }

        public IRTCDTMFSender Dtmf => RTCDTMFSender.Create(((Webrtc.IRTCRtpSender)NativeObject).DtmfSender);

        public IMediaStreamTrack Track => MediaStreamTrack.Create(((Webrtc.IRTCRtpSender)NativeObject).Track);

        public IRTCDtlsTransport Transport => throw new NotImplementedException();

        public RTCRtpCapabilities GetCapabilities(string kind)
        {
            throw new NotImplementedException();
        }

        public RTCRtpSendParameters GetParameters() => ((Webrtc.IRTCRtpSender)NativeObject).Parameters
            .FromNativeToSend();

        public Task<IRTCStatsReport> GetStats()
        {
            throw new NotImplementedException();
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
