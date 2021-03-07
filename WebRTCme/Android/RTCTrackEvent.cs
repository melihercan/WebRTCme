using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;
using System.Linq;

namespace WebRTCme.Android
{
    internal class RTCTrackEvent : ApiBase, IRTCTrackEvent
    {
        private readonly Webrtc.RtpReceiver _nativeRtpReceiver;
        private readonly Webrtc.MediaStream[] _nativeMediaStreams;

        public static IRTCTrackEvent Create(Webrtc.RtpReceiver nativeRtpReceiver, 
            Webrtc.MediaStream[] nativeMediaStreams) =>
                new RTCTrackEvent(nativeRtpReceiver, nativeMediaStreams);

        private RTCTrackEvent(Webrtc.RtpReceiver nativeRtpReceiver, Webrtc.MediaStream[] nativeMediaStreams)
        {
            _nativeRtpReceiver = nativeRtpReceiver;
            _nativeMediaStreams = nativeMediaStreams;
        }

        public IRTCRtpReceiver Receiver => RTCRtpReceiver.Create(_nativeRtpReceiver);

        public IMediaStream[] Streams =>
            _nativeMediaStreams.Select(nativeMediaStream => MediaStream.Create(nativeMediaStream)).ToArray();

        public IMediaStreamTrack Track => MediaStreamTrack.Create(_nativeRtpReceiver.Track());

        public IRTCRtpTransceiver Transceiver => throw new NotImplementedException();

    }
}