using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection, Webrtc.IRTCPeerConnectionDelegate
    {



        public static IRTCPeerConnection Create(RTCConfiguration configuration) => new RTCPeerConnection();

        private RTCPeerConnection() { }


        public event EventHandler OnConnectionStateChanged;
        public event EventHandler OnSignallingStateChange;
        
        public event EventHandler<IMediaStreamEvent> OnAddStream;

        event EventHandler<IRTCPeerConnectionIceEvent> IRTCPeerConnection.OnIceCandidate
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public Task AddIceCandidate(IRTCIceCandidate candidate)
        {
            throw new NotImplementedException();
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream)
        {
            throw new NotImplementedException();
        }

        public void OnIceCandidate(Func<IRTCPeerConnectionIceEvent> callback)
        {
            throw new NotImplementedException();
        }

        public void OnTrack(Func<IRTCTrackEvent> callback)
        {
            throw new NotImplementedException();
        }


        public Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options)
        {
            throw new NotImplementedException();
        }

        
        
        
        
        public void DidChangeSignalingState(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCSignalingState stateChanged)
        {

        }

        public void DidAddStream(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCMediaStream stream)
        {

        }

        public void DidRemoveStream(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCMediaStream stream)
        {

        }

        public void PeerConnectionShouldNegotiate(Webrtc.RTCPeerConnection peerConnection)
        {

        }

        public void DidChangeIceConnectionState(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceConnectionState newState)
        {

        }

        public void DidChangeIceGatheringState(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceGatheringState newState)
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

        }

	}
}
