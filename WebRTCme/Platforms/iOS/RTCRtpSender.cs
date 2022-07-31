using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
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
