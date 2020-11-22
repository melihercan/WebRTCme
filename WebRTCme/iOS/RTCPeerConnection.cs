using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection
    {

        public static Task<IRTCPeerConnection> CreateAsync(RTCConfiguration configuration)
        {
            var ret = new RTCPeerConnection();
            return ret.InitializeAsync(configuration);
        }

        private RTCPeerConnection() { }

        private Task<IRTCPeerConnection> InitializeAsync(RTCConfiguration configuration)
        {
            return Task.FromResult(this as IRTCPeerConnection);
        }

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler OnSignallingStateChange;

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

        public IRTCSessionDescription CreateOffer(RTCOfferOptions options)
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

        Task<IRTCRtpSender> IRTCPeerConnection.AddTrack(IMediaStreamTrack track, IMediaStream stream)
        {
            throw new NotImplementedException();
        }

        Task<IRTCSessionDescription> IRTCPeerConnection.CreateOffer(RTCOfferOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
