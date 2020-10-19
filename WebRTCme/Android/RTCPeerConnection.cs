using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.Android
{
    internal class RTCPeerConnection : IRTCPeerConnection
    {
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
    }
}
