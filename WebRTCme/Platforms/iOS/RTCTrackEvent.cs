using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCTrackEvent : NativeBase<Webrtc.RTCMediaStreamTrack>, IRTCTrackEvent
    {
        private readonly Webrtc.RTCMediaStreamTrack _nativeTrack;

        public static IRTCTrackEvent Create(Webrtc.RTCMediaStreamTrack nativeTrack) => 
            new RTCTrackEvent(nativeTrack);

        public RTCTrackEvent(Webrtc.RTCMediaStreamTrack nativeTrack)
        {
            _nativeTrack = nativeTrack;
        }

        public IRTCRtpReceiver Receiver => throw new NotImplementedException();

        public IMediaStream[] Streams => throw new NotImplementedException();

        public IMediaStreamTrack Track => MediaStreamTrack.Create(_nativeTrack);

        public IRTCRtpTransceiver Transceiver => throw new NotImplementedException();
    }
}
