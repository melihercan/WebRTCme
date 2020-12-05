using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.Android
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection
    {
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
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IMediaStreamEvent> OnAddStream;
        public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        public event EventHandler OnIceConnectionStateChange;
        public event EventHandler OnIceGatheringStateChange;
        public event EventHandler<IRTCIdentityEvent> OnIdentityResult;
        public event EventHandler OnNegotiationNeeded;

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

        event EventHandler<IRTCTrackEvent> IRTCPeerConnection.OnTrack
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

        public void AddStream(IMediaStream mediaStream)
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

        public IRTCSessionDescription CreateOffer(RTCOfferOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<IRTCCertificate> GenerateCertificate()
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

        public RTCIceServer[] GetDefaultIceServers()
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

        public void OnIceCandidate(Func<IRTCPeerConnectionIceEvent> callback)
        {
            throw new NotImplementedException();
        }

        public void OnTrack(Func<IRTCTrackEvent> callback)
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

        Task<IRTCSessionDescription> IRTCPeerConnection.CreateOffer(RTCOfferOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
