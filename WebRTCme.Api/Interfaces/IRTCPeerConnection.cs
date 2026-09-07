using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCPeerConnection : IDisposable // INativeObject
    {
        bool CanTrickleIceCandidates { get; }

        RTCPeerConnectionState ConnectionState { get; }

        RTCSessionDescriptionInit CurrentLocalDescription { get; }

        RTCSessionDescriptionInit CurrentRemoteDescription { get; }

        RTCIceConnectionState IceConnectionState { get; }

        RTCIceGatheringState IceGatheringState { get; }

        RTCSessionDescriptionInit LocalDescription { get; }

        RTCSessionDescriptionInit PendingLocalDescription { get; }

        RTCSessionDescriptionInit PendingRemoteDescription { get; }

        RTCSessionDescriptionInit RemoteDescription { get; }

        IRTCSctpTransport Sctp { get; }

        RTCSignalingState SignalingState { get; }

        event EventHandler OnConnectionStateChanged;
        event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        event EventHandler<IRTCPeerConnectionIceErrorEvent> OnIceCandidateError;
        event EventHandler OnIceConnectionStateChange;
        event EventHandler OnIceGatheringStateChange;
        event EventHandler OnNegotiationNeeded;
        event EventHandler OnSignalingStateChange;
        event EventHandler<IRTCTrackEvent> OnTrack;

        Task AddIceCandidate(RTCIceCandidateInit candidate);

        IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream);

        IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind, RTCRtpTransceiverInit init = null);
        
        IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track, RTCRtpTransceiverInit init = null);

        void Close();

        Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options = null);

        IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options = null);

        Task<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions options = null);

        /*static*/ Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm);

        RTCConfiguration GetConfiguration();

        IRTCRtpReceiver[] GetReceivers();

        IRTCRtpSender[] GetSenders();

        Task<IRTCStatsReport> GetStats();

        IRTCRtpTransceiver[] GetTransceivers();

        void RemoveTrack(IRTCRtpSender sender);

        void RestartIce();

        void SetConfiguration(RTCConfiguration configuration);

        Task SetLocalDescription();

        Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription);

        Task SetRemoteDescription(RTCSessionDescriptionInit sessionDescription);
    }
}
