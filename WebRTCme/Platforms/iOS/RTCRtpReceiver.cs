using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCRtpReceiver : ApiBase, IRTCRtpReceiver
    {
        public static IRTCRtpReceiver Create(Webrtc.IRTCRtpReceiver nativeRtpReceiver) => new RTCRtpReceiver(nativeRtpReceiver);

        private RTCRtpReceiver(Webrtc.IRTCRtpReceiver nativeRtpReceiver) : base(nativeRtpReceiver)  { }

        public IMediaStreamTrack Track => MediaStreamTrack.Create(((Webrtc.IRTCRtpReceiver)NativeObject).Track);

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpContributingSource[] GetContributingSources()
        {
            throw new NotImplementedException();
        }

        public RTCRtpReceiveParameters GetParameters() => ((Webrtc.IRTCRtpReceiver)NativeObject).Parameters
            .FromNativeToReceive();

        public Task<IRTCStatsReport> GetStats()
        {
            throw new NotImplementedException();
        }

        public RTCRtpSynchronizationSource[] GetSynchronizationSources()
        {
            throw new NotImplementedException();
        }

        public RTCRtpCapabilities GetCapabilities(string kind)
        {
            throw new NotImplementedException();
        }
    }
}
