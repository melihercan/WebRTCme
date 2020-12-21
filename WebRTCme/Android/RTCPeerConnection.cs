using Org.Webrtc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection, Webrtc.PeerConnection.IObserver
    {
        public static IRTCPeerConnection Create(RTCConfiguration configuration) =>
            new RTCPeerConnection(configuration);

        private RTCPeerConnection(RTCConfiguration configuration) => 
            NativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory
                .CreatePeerConnection(configuration.ToNative(), this);

        public bool CanTrickleIceCandidates => throw new NotImplementedException();

        public RTCPeerConnectionState ConnectionState =>
            ((Webrtc.PeerConnection)NativeObject).ConnectionState().FromNative();

        public IRTCSessionDescription CurrentRemoteDescription =>
            RTCSessionDescription.Create(((Webrtc.PeerConnection)NativeObject).RemoteDescription);

        public RTCIceConnectionState IceConnectionState =>
            ((Webrtc.PeerConnection)NativeObject).InvokeIceConnectionState().FromNative();

        public RTCIceGatheringState IceGatheringState =>
            ((Webrtc.PeerConnection)NativeObject).InvokeIceGatheringState().FromNative();

        public IRTCSessionDescription LocalDescription =>
            RTCSessionDescription.Create(((Webrtc.PeerConnection)NativeObject).LocalDescription);

        public Task<IRTCIdentityAssertion> PeerIdentity => throw new NotImplementedException();

        public IRTCSessionDescription PendingLocalDescription =>
            RTCSessionDescription.Create(((Webrtc.PeerConnection)NativeObject).LocalDescription);

        public IRTCSessionDescription PendingRemoteDescription =>
            RTCSessionDescription.Create(((Webrtc.PeerConnection)NativeObject).RemoteDescription);

        public IRTCSessionDescription RemoteDescription =>
            RTCSessionDescription.Create(((Webrtc.PeerConnection) NativeObject).RemoteDescription);

        public IRTCSctpTransport Sctp => throw new NotImplementedException();

        public RTCSignalingState SignalingState =>
            ((Webrtc.PeerConnection)NativeObject).InvokeSignalingState().FromNative();

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
            //((Webrtc.PeerConnection)NativeObject).
            throw new NotImplementedException();


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

        public Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm)
        {
            throw new NotImplementedException();
        }

        public RTCConfiguration GetConfiguration()
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

        public void SetConfiguration(RTCConfiguration configuration)
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
        public void OnAddStream(Webrtc.MediaStream p0)
        {
            throw new NotImplementedException();
        }

        public void OnAddTrack(RtpReceiver p0, Webrtc.MediaStream[] p1)
        {
            throw new NotImplementedException();
        }

        void Webrtc.PeerConnection.IObserver.OnDataChannel(DataChannel p0)
        {
            throw new NotImplementedException();
        }

        void Webrtc.PeerConnection.IObserver.OnIceCandidate(IceCandidate p0)
        {
            throw new NotImplementedException();
        }

        public void OnIceCandidatesRemoved(IceCandidate[] p0)
        {
            throw new NotImplementedException();
        }

        public void OnIceConnectionChange(PeerConnection.IceConnectionState p0)
        {
            throw new NotImplementedException();
        }

        public void OnIceConnectionReceivingChange(bool p0)
        {
            throw new NotImplementedException();
        }

        public void OnIceGatheringChange(PeerConnection.IceGatheringState p0)
        {
            throw new NotImplementedException();
        }

        public void OnRemoveStream(Webrtc.MediaStream p0)
        {
            throw new NotImplementedException();
        }

        public void OnRenegotiationNeeded()
        {
            throw new NotImplementedException();
        }

        public void OnSignalingChange(PeerConnection.SignalingState p0)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
