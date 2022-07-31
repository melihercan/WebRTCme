using System;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCRtpReceiver : IDisposable // INativeObject
    {
        IMediaStreamTrack Track { get; }

        IRTCDtlsTransport Transport { get; }

        RTCRtpContributingSource[] GetContributingSources();

        RTCRtpReceiveParameters GetParameters();

        Task<IRTCStatsReport> GetStats();

        RTCRtpSynchronizationSource[] GetSynchronizationSources();

        /*static*/
        RTCRtpCapabilities GetCapabilities(string kind);
    }
}