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
        public void Dispose() { }
        private readonly Webrtc.PeerConnection _nativePeerConnection;

        public RTCRtpReceiver(RtpReceiver nativeReceiver,
            Webrtc.PeerConnection nativePeerConnection = null) : base(nativeReceiver)
        {
            _nativePeerConnection = nativePeerConnection;
        }

        public RTCRtpReceiver(Func<RtpReceiver> nativeReceiverProvider,
            Webrtc.PeerConnection nativePeerConnection = null) : base(nativeReceiverProvider)
        {
            _nativePeerConnection = nativePeerConnection;
        }

        // Resolved through this wrapper rather than captured, so the track survives the
        // re-enumeration that disposes the receiver it came from.
        public IMediaStreamTrack Track => new MediaStreamTrack(() => NativeObject.Track());

        public IRTCDtlsTransport Transport => throw new NotImplementedException();

        public double? JitterBufferTarget
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }


        public RTCRtpContributingSource[] GetContributingSources()
        {
            throw new NotImplementedException();
        }

        public RTCRtpReceiveParameters GetParameters() => NativeObject.Parameters.FromNativeToReceive();

        public Task<IRTCStatsReport> GetStats()
        {
            if (_nativePeerConnection is null)
                throw new InvalidOperationException(
                    "This receiver was not created from a peer connection, so it cannot report stats.");

            var tcs = new TaskCompletionSource<IRTCStatsReport>();
            _nativePeerConnection.GetStats(NativeObject, new StatsExtensions.StatsCollectorProxy(tcs));
            return tcs.Task;
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