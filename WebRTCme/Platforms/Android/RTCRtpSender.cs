using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using System.Threading.Tasks;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCRtpSender : NativeBase<Webrtc.RtpSender>, IRTCRtpSender
    {
        public RTCRtpSender(Webrtc.RtpSender nativeRtpSender) : base(nativeRtpSender)
        {
        }

        public IRTCDTMFSender Dtmf => new RTCDTMFSender(NativeObject.Dtmf());

        public IMediaStreamTrack Track => new MediaStreamTrack(NativeObject.Track());

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpSendParameters GetParameters() => NativeObject.Parameters.FromNativeToSend();

        public Task<IRTCStatsReport> GetStats()
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

        public Task ReplaceTrack(IMediaStreamTrack newTrack = null)
        {
            throw new NotImplementedException();
        }

        public RTCRtpCapabilities GetCapabilities(string kind)
        {
            throw new NotImplementedException();
        }


    }
}