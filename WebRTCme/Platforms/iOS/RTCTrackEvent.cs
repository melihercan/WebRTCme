using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCTrackEvent : ApiBase, IRTCTrackEvent
    {
        private readonly Webrtc.RTCMediaStreamTrack _nativeTrack;

        public static IRTCTrackEvent Create(Webrtc.RTCMediaStreamTrack nativeTrack) => 
            new RTCTrackEvent(nativeTrack);

        private RTCTrackEvent(Webrtc.RTCMediaStreamTrack nativeTrack)
        {
            _nativeTrack = nativeTrack;
        }

        public IRTCRtpReceiver Receiver => throw new NotImplementedException();

        public IMediaStream[] Streams => throw new NotImplementedException();

        public IMediaStreamTrack Track => MediaStreamTrack.Create(_nativeTrack);

        public IRTCRtpTransceiver Transceiver => throw new NotImplementedException();
    }
}
