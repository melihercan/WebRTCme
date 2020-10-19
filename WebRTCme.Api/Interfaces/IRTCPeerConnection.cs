using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCPeerConnection : IAsyncDisposable
    {
        ValueTask<IAsyncDisposable> OnIceCandidate(Func<IRTCPeerConnectionIceEvent, ValueTask> callback);
        ValueTask<IAsyncDisposable> OnTrack(Func<IRTCTrackEvent, ValueTask> callback);
        ValueTask<IRTCRtpSender> AddTrack(IMediaStreamTrack track, IMediaStream stream);
        ValueTask<IRTCSessionDescription> CreateOffer(RTCOfferOptions options);
    }
}
