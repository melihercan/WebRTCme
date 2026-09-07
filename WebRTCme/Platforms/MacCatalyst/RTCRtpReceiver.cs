using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
{
    internal class RTCRtpReceiver : NativeBase<Webrtc.IRTCRtpReceiver>, IRTCRtpReceiver
    {
        public RTCRtpReceiver(Webrtc.IRTCRtpReceiver nativeRtpReceiver) : base(nativeRtpReceiver)  { }

        public IMediaStreamTrack Track => new MediaStreamTrack(NativeObject.Track);

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

        public RTCRtpReceiveParameters GetParameters() => NativeObject.Parameters
            .FromNativeToReceive();

        public Task<IRTCStatsReport> GetStats()
        {
            // The Objective-C category binds statisticsForSender:/statisticsForReceiver: to the
            // generated RTCRtpSender/RTCRtpReceiver classes, but senders and receivers reach us as
            // protocol wrappers, so there is nothing valid to pass. Reaching them needs the binding
            // definition changed to take the protocol interface.
            throw new NotSupportedException(
                "Per-receiver stats are not reachable through the current iOS binding; "
                + "use RTCPeerConnection.GetStats() and select the receiver entries from the report.");
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
