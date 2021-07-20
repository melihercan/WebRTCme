using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace Utilme.SdpTransform
{
    public static class EnumExtensions
    {
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
