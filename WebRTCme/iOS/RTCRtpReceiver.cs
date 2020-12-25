using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCRtpReceiver : ApiBase, IRTCRtpReceiver
    {
        public static IRTCRtpReceiver Create(Webrtc.RTCRtpReceiver nativeRtpReceiver) => new RTCRtpReceiver(nativeRtpReceiver);

        private RTCRtpReceiver(Webrtc.RTCRtpReceiver nativeRtpReceiver) : base(nativeRtpReceiver)  { }

        public IMediaStreamTrack Track => MediaStreamTrack.Create(((Webrtc.RTCRtpReceiver)NativeObject).Track);

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpContributingSource[] GetContributingSources()
        {
            throw new NotImplementedException();
        }

        public RTCRtpReceiveParameters GetParameters() => ((Webrtc.RTCRtpReceiver)NativeObject).Parameters
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
