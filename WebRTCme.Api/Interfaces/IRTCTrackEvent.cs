using System.Dynamic;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCTrackEvent : INativeObject
    {
        IRTCRtpReceiver Receiver { get; set; }

        IMediaStream[] Streams { get; set; }

        IMediaStreamTrack Track { get; set; }

        IRTCRtpTransceiver Transceiver { get; set; }
    }
}