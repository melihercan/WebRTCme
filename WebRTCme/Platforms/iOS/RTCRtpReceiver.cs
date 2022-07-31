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
        public RTCRtpReceiver(Webrtc.IRTCRtpReceiver nativeRtpReceiver) : base(nativeRtpReceiver)  { }

        public IMediaStreamTrack Track => new MediaStreamTrack(NativeObject.Track);

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
