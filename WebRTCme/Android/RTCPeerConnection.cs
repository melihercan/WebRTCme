using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.Android
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection
    {
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


        Task<IRTCSessionDescription> IRTCPeerConnection.CreateOffer(RTCOfferOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
