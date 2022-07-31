using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using System.Threading.Tasks;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCRtpSender : NativeBase<Webrtc.RtpSender>, IRTCRtpSender
    {
        internal static IRTCRtpSender Create(Webrtc.RtpSender nativeRtpSender) =>
            new RTCRtpSender(nativeRtpSender);

        public RTCRtpSender(Webrtc.RtpSender nativeRtpSender) : base(nativeRtpSender)
        {
        }

        public IRTCDTMFSender Dtmf => RTCDTMFSender.Create(((Webrtc.RtpSender)NativeObject).Dtmf());

        public IMediaStreamTrack Track => MediaStreamTrack.Create(((Webrtc.RtpSender)NativeObject).Track());

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpSendParameters GetParameters() => ((Webrtc.RtpSender)NativeObject).Parameters.FromNativeToSend();

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