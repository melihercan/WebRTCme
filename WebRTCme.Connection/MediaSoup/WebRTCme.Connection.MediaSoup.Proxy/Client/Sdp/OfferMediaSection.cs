using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    class OfferMediaSection : MediaSection
    {
        //readonly SctpParameters _sctpParameters;
        //readonly PlainRtpParameters _plainRtpParameters;
        //readonly Mid _mid;
        //readonly MediaKind _kind;
        //RtpParameters

        public OfferMediaSection(
            IceParameters iceParameters,
            IceCandidate[] iceCandidates,
            DtlsParameters dtlsParameters,
            SctpParameters sctpParameters,
            PlainRtpParameters plainRtpParameters,
            bool planB,
            Mid mid,
            MediaKind kind,
            RtpParameters offerRtpParameters,
            string streamId,
            string trackId,
            bool oldDataChannelSpec) : base(iceParameters, iceCandidates, dtlsParameters, planB)
        {
            _mediaObject.Mid = mid;
            _mediaObject.Kind = kind;

            if (plainRtpParameters is not null)
            {
                _mediaObject.Connection = new ConnectionData 
                {
                    Nettype = "IN",
                    AddrType = IpVersion.Ip4.DisplayName(),
                    ConnectionAddress = "127.0.0.1"
                };
                if (sctpParameters is not null)
                    _mediaObject.Protocol = "UDP/TLS/RTP/SAVPF";
                else
                    _mediaObject.Protocol = "UDP/DTLS/SCTP";
                _mediaObject.Port = 7;
            }
            else
            {
                _mediaObject.Connection = new ConnectionData
                {
                    Nettype = "IN",
                    AddrType = plainRtpParameters.IpVersion.DisplayName(),
                    ConnectionAddress = plainRtpParameters.Ip
                };
                _mediaObject.Protocol = "RTP/AVP";
                _mediaObject.Port = plainRtpParameters.Port;
            }

            switch (kind)
            {
                case MediaKind.Audio:
                case MediaKind.Video:
                    {
                        _mediaObject.Direction = Direction.Sendonly;
                        List<Rtpmap> rtpmaps = new();
                        List<RtcpFb> rtcpFbs = new();
                        List<Fmtp> fmtps = new();

                        ////if (!_planB)
                            /////_mediaObject.Ms

                        break;
                    }

                case MediaKind.Application:
                    {
                        break;
                    }
            }
        }


        protected override void SetDtlsRole(DtlsRole? dtlsRole)
        {
        }
    }
}
