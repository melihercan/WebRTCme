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



        public static IRTCPeerConnection Create(RTCConfiguration configuration)
        {
            var ret = new RTCPeerConnection();
            return ret.Initialize(configuration);
        }

        private RTCPeerConnection()
        { 

        }

        private IRTCPeerConnection Initialize(RTCConfiguration configuration)
        {
            var nativeConfiguration = configuration.ToNative();
            NativeObjects.Add(nativeConfiguration);

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
            NativeObjects.Add(nativeConstraints);
            
            SelfNativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory.PeerConnectionWithConfiguration(
                nativeConfiguration,
                nativeConstraints,
                this);

            return this;
        }

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler OnSignallingStateChange;



        public void AddStream(IMediaStream mediaStream)
        {
            throw new NotImplementedException();
        }


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
