using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;
using System.Linq;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCTrackEvent : NativeBase<Webrtc.RtpReceiver>, IRTCTrackEvent
    {
        private readonly Webrtc.RtpReceiver _nativeRtpReceiver;
        private readonly Webrtc.MediaStream[] _nativeMediaStreams;

        public RTCTrackEvent(Webrtc.RtpReceiver nativeRtpReceiver, Webrtc.MediaStream[] nativeMediaStreams)
        {
            _nativeRtpReceiver = nativeRtpReceiver;
            _nativeMediaStreams = nativeMediaStreams;
        }

        public IRTCRtpReceiver Receiver => new RTCRtpReceiver(_nativeRtpReceiver);

        public IMediaStream[] Streams =>
            _nativeMediaStreams.Select(nativeMediaStream => new MediaStream(nativeMediaStream)).ToArray();

        public IMediaStreamTrack Track => new MediaStreamTrack(_nativeRtpReceiver.Track());

        public IRTCRtpTransceiver Transceiver => throw new NotImplementedException();

    }
}