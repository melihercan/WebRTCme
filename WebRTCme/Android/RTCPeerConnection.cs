using Org.Webrtc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection, Webrtc.PeerConnection.IObserver
    {
        private static Webrtc.MediaConstraints NativeDefaultMediaConstraints
        {
            get
            {
                var mandatory = new Dictionary<string, string>
                {
                    ["OfferToReceiveAudio"] = "true",
                    ["OfferToReceiveVideo"] = "true"
                };
                var optional = new Dictionary<string, string>
                {
                    ["DtlsSrtpKeyAgreement"] = "true"
                };
                var nativeConstraints = new Webrtc.MediaConstraints
                {
                    Mandatory = mandatory.Select(p => new Webrtc.MediaConstraints.KeyValuePair(p.Key, p.Value)).ToList(),
                    Optional = optional.Select(p => new Webrtc.MediaConstraints.KeyValuePair(p.Key, p.Value)).ToList()
                };
                return nativeConstraints;
            }
        }


        public static IRTCPeerConnection Create(RTCConfiguration configuration) =>
            new RTCPeerConnection(configuration);

        private RTCPeerConnection(RTCConfiguration configuration) => 
            NativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory
                .CreatePeerConnection(configuration.ToNative(), this);

        public bool CanTrickleIceCandidates => throw new NotImplementedException();

        public RTCPeerConnectionState ConnectionState =>
            ((Webrtc.PeerConnection)NativeObject).ConnectionState().FromNative();

        public IRTCSessionDescription CurrentLocalDescription =>
            RTCSessionDescription.Create(((Webrtc.PeerConnection)NativeObject).LocalDescription);

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
        public event EventHandler OnNegotiationNeeded;
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IRTCTrackEvent> OnTrack;

        public RTCIceServer[] GetDefaultIceServers() =>
            throw new NotImplementedException();


        public Task AddIceCandidate(IRTCIceCandidate candidate)
        {
            ((Webrtc.PeerConnection)NativeObject).AddIceCandidate(candidate.NativeObject as Webrtc.IceCandidate);
            return Task.CompletedTask;
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream) =>
            RTCRtpSender.Create(((Webrtc.PeerConnection)NativeObject).AddTrack(
                track.NativeObject as Webrtc.MediaStreamTrack, new List<string> { stream.Id }));


        public void Close() => ((Webrtc.PeerConnection)NativeObject).Close();

        public Task<IRTCSessionDescription> CreateAnswer(RTCAnswerOptions options)
        {
            var tcs = new TaskCompletionSource<IRTCSessionDescription>();
            ((Webrtc.PeerConnection)NativeObject).CreateAnswer(
                new SdpObserverProxy(tcs), NativeDefaultMediaConstraints);
            return tcs.Task;
        }

        public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options) =>
            RTCDataChannel.Create(((Webrtc.PeerConnection)NativeObject).CreateDataChannel(label, options.ToNative()));

        public Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options)
        {
            var tcs = new TaskCompletionSource<IRTCSessionDescription>();
            ((Webrtc.PeerConnection)NativeObject).CreateOffer(
                new SdpObserverProxy(tcs), NativeDefaultMediaConstraints);
            return tcs.Task;
        }

        public Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm) =>
            //// TODO: How to use keygenAlgorithm
            Task.FromResult(RTCCertificate.Create(Webrtc.RtcCertificatePem.GenerateCertificate()));

        public RTCConfiguration GetConfiguration()
        {
            //// TODO: HOW TO GET Configuration??? Requires for IceServers too.
            throw new NotImplementedException();
        }

        public void GetIdentityAssertion()
        {
            throw new NotImplementedException();
        }

        public IRTCRtpReceiver[] GetReceivers() =>
            ((Webrtc.PeerConnection)NativeObject).Receivers
                .Select(nativeReceiver => RTCRtpReceiver.Create(nativeReceiver)).ToArray();

        public IRTCRtpSender[] GetSenders() =>
            ((Webrtc.PeerConnection)NativeObject).Senders
                .Select(nativeSender => RTCRtpSender.Create(nativeSender)).ToArray();


        public Task<IRTCStatsReport> GetStats() //// TODO: REWORK STATS
        {
            throw new NotImplementedException();
        }

        public IRTCRtpTransceiver[] GetTransceivers() =>
            ((Webrtc.PeerConnection)NativeObject).Transceivers
                .Select(nativeTransceiver => RTCRtpTransceiver.Create(nativeTransceiver)).ToArray();


        public void RemoveTrack(IRTCRtpSender sender) =>
            ((Webrtc.PeerConnection)NativeObject).RemoveTrack(sender.NativeObject as Webrtc.RtpSender);

        public void RestartIce()
        {
            throw new NotImplementedException();
        }

        public void SetConfiguration(RTCConfiguration configuration) =>
            ((Webrtc.PeerConnection)NativeObject).SetConfiguration(configuration.ToNative());

        public void SetIdentityProvider(string domainName, string protocol = null, string userName = null)
        {
            throw new NotImplementedException();
        }

        public Task SetLocalDescription(IRTCSessionDescription sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>();
            ((Webrtc.PeerConnection)NativeObject).SetLocalDescription(
                new SdpObserverProxy(tcs), sessionDescription.NativeObject as Webrtc.SessionDescription);
            return tcs.Task;
        }

        public Task SetRemoteDescription(IRTCSessionDescription sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>();
            ((Webrtc.PeerConnection)NativeObject).SetRemoteDescription(
                new SdpObserverProxy(tcs), sessionDescription.NativeObject as Webrtc.SessionDescription);
            return tcs.Task;
        }




        #region NativeEvents
        public void OnAddStream(Webrtc.MediaStream p0)
        {
            // Depreceted.
        }

        public void OnAddTrack(RtpReceiver p0, Webrtc.MediaStream[] p1) => 
            OnTrack?.Invoke(this, RTCTrackEvent.Create(p0, p1));

        void Webrtc.PeerConnection.IObserver.OnDataChannel(DataChannel p0) =>
            OnDataChannel?.Invoke(this, RTCDataChannelEvent.Create(p0));

        void Webrtc.PeerConnection.IObserver.OnIceCandidate(IceCandidate p0)
        {
            OnIceCandidate?.Invoke(this, RTCPeerConnectionIceEvent.Create(p0));
        }

        public void OnIceCandidatesRemoved(IceCandidate[] p0)
        {
        }

        public void OnIceConnectionChange(PeerConnection.IceConnectionState p0) =>
            OnIceConnectionStateChange?.Invoke(this, EventArgs.Empty);

        public void OnIceConnectionReceivingChange(bool p0)
        {

        }

        public void OnIceGatheringChange(PeerConnection.IceGatheringState p0) =>
            OnIceGatheringStateChange?.Invoke(this, EventArgs.Empty);

        public void OnRemoveStream(Webrtc.MediaStream p0)
        {
            // Depreceted.
        }

        public void OnRenegotiationNeeded() => OnNegotiationNeeded?.Invoke(this, EventArgs.Empty);

        public void OnSignalingChange(PeerConnection.SignalingState p0) =>
            OnSignallingStateChange?.Invoke(this, EventArgs.Empty);
        
        #endregion


        #region SdpObserver
        private class SdpObserverProxy : Java.Lang.Object, Webrtc.ISdpObserver
        {
            private readonly TaskCompletionSource<IRTCSessionDescription> _tcsCreate;
            private readonly TaskCompletionSource<object> _tcsSet;

            public SdpObserverProxy(TaskCompletionSource<IRTCSessionDescription> tcs) => _tcsCreate = tcs;

            public SdpObserverProxy(TaskCompletionSource<object> tcs) => _tcsSet = tcs;

            public void OnCreateFailure(string p0) => _tcsCreate?.SetException(new Exception($"{p0}"));

            public void OnCreateSuccess(SessionDescription p0) => 
                _tcsCreate?.SetResult(RTCSessionDescription.Create(p0));

            public void OnSetFailure(string p0) => _tcsSet?.SetException(new Exception($"{p0}"));

            public void OnSetSuccess() => _tcsSet?.SetResult(null);
        }
        #endregion

    }


}
