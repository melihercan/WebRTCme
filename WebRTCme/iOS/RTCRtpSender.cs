using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCRtpSender : ApiBase, IRTCRtpSender
    {
        public object NativeObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IRTCDTMFSender Dtmf => throw new NotImplementedException();

        public IMediaStreamTrack Track => throw new NotImplementedException();

        public IRTCDtlsTransport Transport => throw new NotImplementedException();

        public RTCRtpCapabilities GetCapabilities(string kind)
        {
            throw new NotImplementedException();
        }

        public RTCRtpSendParameters GetParameters()
        {
            throw new NotImplementedException();
        }

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
