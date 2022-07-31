using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;
using System.Threading.Tasks;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCRtpReceiver : NativeBase<Webrtc.RtpReceiver>, IRTCRtpReceiver
    {
        public RTCRtpReceiver(RtpReceiver nativeReceiver) : base(nativeReceiver)
        {
        }

        public IMediaStreamTrack Track => new MediaStreamTrack(NativeObject.Track());

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpContributingSource[] GetContributingSources()
        {
            throw new NotImplementedException();
        }

        public RTCRtpReceiveParameters GetParameters() => NativeObject.Parameters.FromNativeToReceive();

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