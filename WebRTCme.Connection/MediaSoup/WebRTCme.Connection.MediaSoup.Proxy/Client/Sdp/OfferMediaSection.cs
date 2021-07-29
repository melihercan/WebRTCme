using System;
using System.Collections.Generic;
using System.Linq;
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

                        if (!_planB)
                            _mediaObject.Msid = new() 
                            { 
                                Id = streamId ?? "-",
                                AppData = trackId
                            };

                        foreach (var codec in offerRtpParameters.Codecs)
                        {
                            Rtpmap rtpmap = new() 
                            {
                                PayloadType = codec.PayloadType,
                                EncodingName = GetCodecName(codec),
                                ClockRate = codec.ClockRate,
                                Channels = codec.Channels > 1 ? codec.Channels : null
                            };
                            rtpmaps.Add(rtpmap);

                            Fmtp fmtp = new()
                            {
                                PayloadType = codec.PayloadType,
                                Value = string.Empty
                            };
                            foreach (var key in codec.Parameters.Keys)
                            {
                                if (!string.IsNullOrEmpty(fmtp.Value))
                                    fmtp.Value += ";";
                                fmtp.Value += $"{key}={codec.Parameters[key]}";
                            }
                            if (!string.IsNullOrEmpty(fmtp.Value))
                                fmtps.Add(fmtp);

                            foreach (var fb in codec.RtcpFeedback)
                            {
                                RtcpFb rtcpFb = new() 
                                {
                                    PayloadType = codec.PayloadType,
                                    Type = fb.Type,
                                    SubType = fb.Parameter
                                };
                                rtcpFbs.Add(rtcpFb);
                            }
                        }
                        _mediaObject.Rtpmaps = rtpmaps.ToArray();
                        _mediaObject.RtcpFbs = rtcpFbs.ToArray();
                        _mediaObject.Fmtps = fmtps.ToArray();

                        _mediaObject.Payloads = string.Join(" ", offerRtpParameters.Codecs
                            .Select(codec => codec.PayloadType.ToString()).ToArray());


                        List<RtpHeaderExtensionParameters> extensions = new();
                        foreach (var headerExtension in offerRtpParameters.HeaderExtensions)
                        {
                            var ext = new RtpHeaderExtensionParameters 
                            {
                                Uri = headerExtension.Uri,
                                Number = headerExtension.Number
                            };
                            extensions.Add(ext);
                        }
                        _mediaObject.Extensions = extensions.ToArray();

                        _mediaObject.BinaryAttributes.RtcpMux = true;
                        _mediaObject.BinaryAttributes.RtcpRsize = true;

                        var encoding = offerRtpParameters.Encodings[0];
                        var ssrc = encoding.Ssrc;
                        var rtxSsrc = (encoding.Rtx is not null && encoding.Rtx.Ssrc.HasValue) ? 
                            encoding.Rtx.Ssrc : null;

                        List<Ssrc> ssrcs = new();
                        List<SsrcGroup> ssrcGroups = new();

                        if (offerRtpParameters.Rtcp.Cname is not null)
                        {
                            ssrcs.Add(new Ssrc
                            {
                                Id = (uint)ssrc,
                                AttributesAndValues = new string[] { $"cname:{offerRtpParameters.Rtcp.Cname}" }
                            }); 
                        }

                        if (_planB)
                        {
                            ssrcs.Add(new Ssrc
                            {
                                Id = (uint)ssrc,
                                AttributesAndValues = new string[] { $"msid:{streamId ?? "-"} {trackId}" }
                            });
                        }

                        if (rtxSsrc.HasValue)
                        {
                            if (offerRtpParameters.Rtcp.Cname is not null)
                            {
                                ssrcs.Add(new Ssrc
                                {
                                    Id = (uint)rtxSsrc,
                                    AttributesAndValues = new string[] { $"cname:{offerRtpParameters.Rtcp.Cname}" }
                                });
                            }

                            if (_planB)
                            {
                                ssrcs.Add(new Ssrc
                                {
                                    Id = (uint)rtxSsrc,
                                    AttributesAndValues = new string[] { $"msid:{streamId ?? "-"} {trackId}" }
                                });
                            }

                            // Associate original and retransmission SSRCs.
                            ssrcGroups.Add(new SsrcGroup
                            {
                                Semantics = "FID",
                                SsrcIds = new string[] { $"{ssrc} {rtxSsrc}"}
                            });
                        }

                        _mediaObject.Ssrcs = ssrcs.ToArray();
                        _mediaObject.SsrcGroups = ssrcGroups.ToArray();

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
