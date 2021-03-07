using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;
using System.Threading.Tasks;

namespace WebRTCme.Android
{
    internal class RTCRtpReceiver : ApiBase, IRTCRtpReceiver
    {
        public static IRTCRtpReceiver Create(Webrtc.RtpReceiver nativeReceiver) =>
            new RTCRtpReceiver(nativeReceiver);

        public RTCRtpReceiver(RtpReceiver nativeReceiver) : base(nativeReceiver)
        {
        }

        public IMediaStreamTrack Track => MediaStreamTrack.Create(((Webrtc.RtpReceiver)NativeObject).Track());

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpContributingSource[] GetContributingSources()
        {
            throw new NotImplementedException();
        }

        public RTCRtpReceiveParameters GetParameters() => ((Webrtc.RtpReceiver)NativeObject).Parameters.FromNativeToReceive();

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