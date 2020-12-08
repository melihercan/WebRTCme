using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection, Webrtc.IRTCPeerConnectionDelegate
    {
        public static IRTCPeerConnection Create(IRTCConfiguration configuration) => new RTCPeerConnection(configuration);

        private RTCPeerConnection(IRTCConfiguration configuration)
        {
            var nativeConfiguration = configuration.NativeObject as Webrtc.RTCConfiguration;

            var mandatory = new Dictionary<string, string>
            {
                ["OfferToReceiveAudio"] = "true",
                ["OfferToReceiveVideo"] = "true"
            };
            var optional = new Dictionary<string, string>
            {
                ["DtlsSrtpKeyAgreement"] = "true"
            };
            var nativeConstraints = new Webrtc.RTCMediaConstraints(
                NSDictionary<NSString, NSString>.FromObjectsAndKeys(
                    mandatory.Values.ToArray(), mandatory.Keys.ToArray()),
                NSDictionary<NSString, NSString>.FromObjectsAndKeys(
                    optional.Values.ToArray(), optional.Keys.ToArray()));

            NativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory.PeerConnectionWithConfiguration(
                nativeConfiguration,
                nativeConstraints,
                this);
        }

        public bool CanTrickleIceCandidates => throw new NotImplementedException();

        public RTCPeerConnectionState ConnectionState => throw new NotImplementedException();

        public IRTCSessionDescription CurrentRemoteDescription => throw new NotImplementedException();

        public RTCIceConnectionState IceConnectionState => throw new NotImplementedException();

        public RTCIceGatheringState IceGatheringState => throw new NotImplementedException();

        public IRTCSessionDescription LocalDescription => throw new NotImplementedException();

        public Task<IRTCIdentityAssertion> PeerIdentity => throw new NotImplementedException();

        public IRTCSessionDescription PendingLocalDescription => throw new NotImplementedException();

        public IRTCSessionDescription PendingRemoteDescription => throw new NotImplementedException();

        public IRTCSessionDescription RemoteDescription => throw new NotImplementedException();

        public IRTCSctpTransport Sctp => throw new NotImplementedException();

        public RTCSignallingState SignallingState => throw new NotImplementedException();

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        public event EventHandler OnIceConnectionStateChange;
        public event EventHandler OnIceGatheringStateChange;
        public event EventHandler<IRTCIdentityEvent> OnIdentityResult;
        public event EventHandler OnNegotiationNeeded;
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IRTCTrackEvent> OnTrack;


        public RTCIceServer[] GetDefaultIceServers()
        {
            throw new NotImplementedException();
        }

        public Task AddIceCandidate(IRTCIceCandidate candidate)
        {
            throw new NotImplementedException();
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public Task<IRTCSessionDescription> CreateAnswer(RTCAnswerOptions options = null)
        {
            throw new NotImplementedException();
        }

        public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm)
        {
            throw new NotImplementedException();
        }

        public IRTCConfiguration GetConfiguration()
        {
            throw new NotImplementedException();
        }

        public void GetIdentityAssertion()
        {
            throw new NotImplementedException();
        }

        public IRTCRtpReceiver[] GetReceivers()
        {
            throw new NotImplementedException();
        }

        public IRTCRtpSender[] GetSenders()
        {
            throw new NotImplementedException();
        }

        public Task<IRTCStatsReport> GetStats()
        {
            throw new NotImplementedException();
        }

        public IRTCRtpTransceiver[] GetTransceivers()
        {
            throw new NotImplementedException();
        }

        public void RemoveTrack(IRTCRtpSender sender)
        {
            throw new NotImplementedException();
        }

        public void RestartIce()
        {
            throw new NotImplementedException();
        }

        public void SetConfiguration(IRTCConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public void SetIdentityProvider(string domainName, string protocol = null, string userName = null)
        {
            throw new NotImplementedException();
        }

        public Task SetLocalDescription(IRTCSessionDescription sessionDescription)
        {
            throw new NotImplementedException();
        }

        public Task SetRemoteDescription(IRTCSessionDescription sessionDescription)
        {
            throw new NotImplementedException();
        }

        
        
        
        #region NativeEvents
        public void DidChangeSignalingState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCSignalingState stateChanged)
        {

        }

        public void DidAddStream(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCMediaStream stream)
        {
            // Depreceted
        }

        public void DidRemoveStream(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCMediaStream stream)
        {
            // Depreceted
        }

        public void PeerConnectionShouldNegotiate(Webrtc.RTCPeerConnection peerConnection)
        {

        }

        public void DidChangeIceConnectionState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCIceConnectionState newState)
        {

        }

        public void DidChangeIceGatheringState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCIceGatheringState newState)
        {

        }

        public void DidGenerateIceCandidate(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate candidate)
        {

        }

        public void DidRemoveIceCandidates(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate[] candidates)
        {

        }

        public void DidOpenDataChannel(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCDataChannel dataChannel)
        {
            OnDataChannel?.Invoke(this, RTCDataChannelEvent.Create(dataChannel));
        }

        public void DidChangeStandardizedIceConnectionState(Webrtc.RTCPeerConnection peerConnection,
            Webrtc.RTCIceConnectionState newState)
        {

        }

        public void DidChangeConnectionState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCPeerConnectionState newState)
        {
            OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DidStartReceivingOnTransceiver(Webrtc.RTCPeerConnection peerConnection,
            Webrtc.RTCRtpTransceiver transceiver)
        {

        }

        public void DidAddReceiver(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCRtpReceiver rtpReceiver,
            Webrtc.RTCMediaStream[] mediaStreams)
        {

        }

        public void DidRemoveReceiver(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCRtpReceiver rtpReceiver)
        {

        }

        public void DidChangeLocalCandidate(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate local,
            Webrtc.RTCIceCandidate remote, int lastDataReceivedMs, string reason)
        {

        }


        #endregion
    }
}
