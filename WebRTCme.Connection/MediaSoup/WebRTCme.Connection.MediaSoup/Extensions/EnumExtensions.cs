using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup
{
    public static class EnumExtensions
    {
        public static MediaKind ToMediaSoup(this MediaStreamTrackKind kind) =>
            kind switch
            {
                MediaStreamTrackKind.Audio => MediaKind.Audio,
                MediaStreamTrackKind.Video => MediaKind.Video,
                _ => throw new NotImplementedException()
            };

        public static MediaType ToSdp(this MediaKind kind) =>
            kind switch
            {
                MediaKind.Audio => MediaType.Audio,
                MediaKind.Video => MediaType.Video,
                MediaKind.Application => MediaType.Application,
                _ => throw new NotImplementedException()
            };

        public static MediaKind ToMediaSoup(this MediaType type_) =>
            type_ switch
            {
                MediaType.Audio => MediaKind.Audio,
                MediaType.Video => MediaKind.Video,
                MediaType.Application => MediaKind.Application,
                _ => throw new NotImplementedException()
            };

        public static CandidateTransport ToSdp(this RTCIceProtocol protocol) =>
            protocol switch
            {
                RTCIceProtocol.Udp => CandidateTransport.Udp,
                RTCIceProtocol.Tcp => CandidateTransport.Tcp,
                _ => throw new NotImplementedException(),
            };

        public static CandidateType ToSdp(this RTCIceCandidateType type) =>
            type switch
            {
                RTCIceCandidateType.Host => CandidateType.Host,
                RTCIceCandidateType.Srflx => CandidateType.Srflx,
                RTCIceCandidateType.Prflx => CandidateType.Prflx,
                RTCIceCandidateType.Relay => CandidateType.Relay,
                _ => throw new NotImplementedException(),
            };

    }
}
