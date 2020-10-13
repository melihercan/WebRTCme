using System.Dynamic;
using WebRTCme.Interfaces;

namespace WebRTCme
{
    public class RTCTrackEvent
    {
        public RTCRtpReceiver Receiver { get; set; }
        public IMediaStream[] Streams { get; set; }
        public IMediaStreamTrack Track { get; set; }
        public IRTCRtpTransceiver Transceiver { get; set; }
    }
}