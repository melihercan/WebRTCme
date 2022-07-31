using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCRtpReceiver : NativeBase<Webrtc.IRTCRtpReceiver>, IRTCRtpReceiver
    {
        public static IRTCRtpReceiver Create(Webrtc.IRTCRtpReceiver nativeRtpReceiver) => new RTCRtpReceiver(nativeRtpReceiver);

        public RTCRtpReceiver(Webrtc.IRTCRtpReceiver nativeRtpReceiver) : base(nativeRtpReceiver)  { }

        public IMediaStreamTrack Track => MediaStreamTrack.Create(NativeObject.Track);

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpContributingSource[] GetContributingSources()
        {
            throw new NotImplementedException();
        }

        public RTCRtpReceiveParameters GetParameters() => NativeObject.Parameters
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
