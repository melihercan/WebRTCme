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
        ValueTask<IAsyncDisposable> OnIceCandidateAsync(Func<IRTCPeerConnectionIceEvent, ValueTask> callback);
        ValueTask<IAsyncDisposable> OnTrackAsync(Func<IRTCTrackEvent, ValueTask> callback);
        ValueTask<IRTCRtpSender> AddTrackAsync(IMediaStreamTrackAsync track, IMediaStreamAsync stream);
        ValueTask<IRTCSessionDescription> CreateOfferAsync(RTCOfferOptions options);
    }

}
