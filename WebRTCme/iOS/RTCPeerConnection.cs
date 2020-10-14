using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCPeerConnection : IRTCPeerConnection
    {
        public ValueTask<IRTCRtpSender> AddTrack(IMediaStreamTrack track, IMediaStream[] stream)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IRTCRtpSender> AddTrack(IMediaStreamTrack track, IMediaStream stream)
        {
            throw new NotImplementedException();
        }

        public ValueTask<RTCSessionDescription> CreateOffer(RTCOfferOptions options)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public void Do()
        {
            Console.WriteLine("I am iOS");
        }

        public ValueTask<IAsyncDisposable> OnIceCandidate(Func<RTCPeerConnectionIceEvent, ValueTask> callback)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IAsyncDisposable> OnTrack(Func<RTCTrackEvent, ValueTask> callback)
        {
            throw new NotImplementedException();
        }
    }
}
