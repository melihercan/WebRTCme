using System.Dynamic;
using System.Threading.Tasks;
using WebRTCme.Interfaces;

namespace WebRTCme
{
    public interface IRTCTrackEvent
    {
        Task<IRTCRtpReceiver> Receiver { get; set; }

        Task<IMediaStream[]> Streams { get; set; }

        Task<IMediaStreamTrack> Track { get; set; }

        Task<IRTCRtpTransceiver> Transceiver { get; set; }
    }
}