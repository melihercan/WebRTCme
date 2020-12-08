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

        public bool CanTrickleIceCandidates => throw new NotSupportedException();

        public RTCPeerConnectionState ConnectionState =>
            ((Webrtc.RTCPeerConnection)NativeObject).ConnectionState.FromNative();

        public IRTCSessionDescription CurrentRemoteDescription =>
            RTCSessionDescription.Create(((Webrtc.RTCPeerConnection)NativeObject).RemoteDescription);

        public RTCIceConnectionState IceConnectionState =>
            ((Webrtc.RTCPeerConnection)NativeObject).IceConnectionState.FromNative();

        public RTCIceGatheringState IceGatheringState =>
            ((Webrtc.RTCPeerConnection)NativeObject).IceGatheringState.FromNative();

        public IRTCSessionDescription LocalDescription =>
            RTCSessionDescription.Create(((Webrtc.RTCPeerConnection)NativeObject).LocalDescription);

        public Task<IRTCIdentityAssertion> PeerIdentity => throw new NotImplementedException();

        public IRTCSessionDescription PendingLocalDescription =>
            RTCSessionDescription.Create(((Webrtc.RTCPeerConnection)NativeObject).LocalDescription);

        public IRTCSessionDescription PendingRemoteDescription =>
            RTCSessionDescription.Create(((Webrtc.RTCPeerConnection)NativeObject).RemoteDescription);

        public IRTCSessionDescription RemoteDescription =>
            RTCSessionDescription.Create(((Webrtc.RTCPeerConnection)NativeObject).RemoteDescription);


        public IRTCSctpTransport Sctp => throw new NotImplementedException();

        public RTCSignalingState SignalingState =>
            ((Webrtc.RTCPeerConnection)NativeObject).SignalingState.FromNative();

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        public event EventHandler OnIceConnectionStateChange;
        public event EventHandler OnIceGatheringStateChange;
        public event EventHandler<IRTCIdentityEvent> OnIdentityResult;
        public event EventHandler OnNegotiationNeeded;
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IRTCTrackEvent> OnTrack;

        public RTCIceServer[] GetDefaultIceServers() =>
            ((Webrtc.RTCPeerConnection)NativeObject).Configuration.IceServers
                .Select(nativeIceServer => nativeIceServer.FromNative())
                .ToArray();

        public Task AddIceCandidate(IRTCIceCandidate candidate)
        {
            ((Webrtc.RTCPeerConnection)NativeObject).AddIceCandidate(candidate.NativeObject as Webrtc.RTCIceCandidate);
            return Task.CompletedTask;
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream) =>
            RTCRtpSender.Create(((Webrtc.RTCPeerConnection)NativeObject).AddTrack(
                track.NativeObject as Webrtc.RTCMediaStreamTrack, new string[] {stream.Id}));

        public void Close() =>
            ((Webrtc.RTCPeerConnection)NativeObject).Close();

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
            OnSignallingStateChange?.Invoke(this, EventArgs.Empty);
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
            OnNegotiationNeeded?.Invoke(this, EventArgs.Empty);
        }

        public void DidChangeIceConnectionState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCIceConnectionState newState)
        {
            OnIceConnectionStateChange?.Invoke(this, EventArgs.Empty);
        }

        public void DidChangeIceGatheringState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCIceGatheringState newState)
        {
            OnIceGatheringStateChange?.Invoke(this, EventArgs.Empty);
        }

        public void DidGenerateIceCandidate(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate candidate)
        {
            OnIceCandidate?.Invoke(this, RTCPeerConnectionIceEvent.Create(candidate));
        }

        public void DidRemoveIceCandidates(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate[] candidates)
        {
            //// TODO: Anything to do for removal???
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
