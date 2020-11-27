using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{

    public interface IRTCPeerConnection : INativeObject
    {
        Task AddIceCandidate(IRTCIceCandidate candidate);


        IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream);

        Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options);


        void AddStream(IMediaStream mediaStream);

        event EventHandler<IMediaStreamEvent> OnAddStream;

        event EventHandler OnConnectionStateChanged;

        ////event EventHandler<IRTCDataChannelEvent> OnDataChannel;

        event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;

        ////event EventHandler OnIceConnectionStateChange;
        ////event EventHandler OnIceGatheringStateChange;
        ////event EventHandler<I???> OnIdentityResult;
        ////event EventHandler OnNegotiationNeeded;

        event EventHandler OnSignallingStateChange;

        ////event EventHandler<IRTCTrackEvent???  IMediaStreamTrackEvent???> OnTrack;

    }

}
