using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{

    public interface IRTCPeerConnection : INativeObject
    {
        bool CanTrickleIceCandidates { get; }

        RTCPeerConnectionState ConnectionState { get; }

        IRTCSessionDescription CurrentRemoteDescription { get; }

        RTCIceConnectionState IceConnectionState { get; }

        RTCIceGatheringState IceGatheringState { get; }

        IRTCSessionDescription LocalDescription { get; }

        Task<IRTCIdentityAssertion> PeerIdentity { get; }

        IRTCSessionDescription PendingLocalDescription { get; }

        IRTCSessionDescription PendingRemoteDescription { get; }

        IRTCSessionDescription RemoteDescription { get; }

        IRTCSctpTransport Sctp { get; }

        RTCSignallingState SignallingState { get; }


        event EventHandler OnConnectionStateChanged;
        event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        event EventHandler OnIceConnectionStateChange;
        event EventHandler OnIceGatheringStateChange;
        event EventHandler<IRTCIdentityEvent> OnIdentityResult;
        event EventHandler OnNegotiationNeeded;
        event EventHandler OnSignallingStateChange;
        event EventHandler<IRTCTrackEvent> OnTrack;

        RTCIceServer[] GetDefaultIceServers();
        
        Task AddIceCandidate(IRTCIceCandidate candidate);

        IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream);

        void Close();

        Task<IRTCSessionDescription> CreateAnswer(RTCAnswerOptions options = null);

        IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options = null);

        Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options = null);

        /*static*/ Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm);

        RTCConfiguration GetConfiguration();

        void GetIdentityAssertion();

        IRTCRtpReceiver[] GetReceivers();

        IRTCRtpSender[] GetSenders();

        Task<IRTCStatsReport> GetStats();

        IRTCRtpTransceiver[] GetTransceivers();

        void RemoveTrack(IRTCRtpSender sender);

        void RestartIce();

        void SetConfiguration(RTCConfiguration configuration);

        void SetIdentityProvider(string domainName, string protocol = null, string userName = null);

        Task SetLocalDescription(IRTCSessionDescription sessionDescription);

        Task SetRemoteDescription(IRTCSessionDescription sessionDescription);
    }

}
