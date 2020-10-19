using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCPeerConnection
    {
        IAsyncDisposable OnIceCandidate(Func<IRTCPeerConnectionIceEvent, ValueTask> callback);
        IAsyncDisposable OnTrack(Func<IRTCTrackEvent, ValueTask> callback);
        IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream);
        IRTCSessionDescription CreateOffer(RTCOfferOptions options);
    }

    public interface IRTCPeerConnectionAsync : IAsyncDisposable
    {
        Task<IAsyncDisposable> OnIceCandidateAsync(Func<IRTCPeerConnectionIceEvent, ValueTask> callback);
        Task<IAsyncDisposable> OnTrackAsync(Func<IRTCTrackEvent, ValueTask> callback);
        Task<IRTCRtpSender> AddTrackAsync(IMediaStreamTrackAsync track, IMediaStreamAsync stream);
        Task<IRTCSessionDescription> CreateOfferAsync(RTCOfferOptions options);
    }

}
