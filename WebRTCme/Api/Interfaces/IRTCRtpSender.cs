using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCRtpSender : IDisposable // INativeObject
    {
        IRTCDTMFSender Dtmf { get; }

        IMediaStreamTrack Track { get; }

        IRTCDtlsTransport Transport { get; }

        RTCRtpSendParameters GetParameters();

        Task<IRTCStatsReport> GetStats();

        Task SetParameters(RTCRtpSendParameters parameters);

        void SetStreams(IMediaStream[] mediaStreams);

        Task ReplaceTrack(IMediaStreamTrack newTrack = null);

        /*static*/
        RTCRtpCapabilities GetCapabilities(string kind);
    }
}
