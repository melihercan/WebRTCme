using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using System.Threading.Tasks;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCRtpSender : NativeBase<Webrtc.RtpSender>, IRTCRtpSender
    {
        public void Dispose() { }
        private readonly Webrtc.PeerConnection _nativePeerConnection;

        public RTCRtpSender(Webrtc.RtpSender nativeRtpSender,
            Webrtc.PeerConnection nativePeerConnection = null) : base(nativeRtpSender)
        {
            _nativePeerConnection = nativePeerConnection;
        }

        public RTCRtpSender(Func<Webrtc.RtpSender> nativeRtpSenderProvider,
            Webrtc.PeerConnection nativePeerConnection = null) : base(nativeRtpSenderProvider)
        {
            _nativePeerConnection = nativePeerConnection;
        }

        public IRTCDTMFSender Dtmf => new RTCDTMFSender(NativeObject.Dtmf());

        public IMediaStreamTrack Track => new MediaStreamTrack(() => NativeObject.Track());

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpSendParameters GetParameters() => NativeObject.Parameters.FromNativeToSend();

        public Task<IRTCStatsReport> GetStats()
        {
            if (_nativePeerConnection is null)
                throw new InvalidOperationException(
                    "This sender was not created from a peer connection, so it cannot report stats.");

            var tcs = new TaskCompletionSource<IRTCStatsReport>();
            _nativePeerConnection.GetStats(NativeObject, new StatsExtensions.StatsCollectorProxy(tcs));
            return tcs.Task;
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