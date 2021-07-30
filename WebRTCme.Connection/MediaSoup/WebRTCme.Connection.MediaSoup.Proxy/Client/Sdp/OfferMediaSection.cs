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
                            _mediaObject.Rtpmaps.Add(rtpmap);

                            Fmtp fmtp = new()
                            {
                                PayloadType = codec.PayloadType,
                                Value = string.Empty
                            };
                            //foreach (var key in codec.Parameters.Keys)
                            //{
                            //    if (!string.IsNullOrEmpty(fmtp.Value))
                            //        fmtp.Value += ";";
                            //    fmtp.Value += $"{key}={codec.Parameters[key]}";
                            //}
                            if (!string.IsNullOrEmpty(fmtp.Value))
                                _mediaObject.Fmtps.Add(fmtp);

                            foreach (var fb in codec.RtcpFeedback)
                            {
                                RtcpFb rtcpFb = new() 
                                {
                                    PayloadType = codec.PayloadType,
                                    Type = fb.Type,
                                    SubType = fb.Parameter
                                };
                                _mediaObject.RtcpFbs.Add(rtcpFb);
                            }
                        }

                        _mediaObject.Payloads += string.Join(" ", offerRtpParameters.Codecs
                            .Select(codec => codec.PayloadType.ToString()));


                        foreach (var headerExtension in offerRtpParameters.HeaderExtensions)
                        {
                            var ext = new RtpHeaderExtensionParameters 
                            {
                                Uri = headerExtension.Uri,
                                Number = headerExtension.Number
                            };
                            _mediaObject.Extensions.Add(ext);
                        }

                        _mediaObject.BinaryAttributes.RtcpMux = true;
                        _mediaObject.BinaryAttributes.RtcpRsize = true;

                        var encoding = offerRtpParameters.Encodings[0];
                        var ssrc = encoding.Ssrc;
                        var rtxSsrc = (encoding.Rtx is not null && encoding.Rtx.Ssrc.HasValue) ? 
                            encoding.Rtx.Ssrc : null;

                        if (offerRtpParameters.Rtcp.Cname is not null)
                        {
                            _mediaObject.Ssrcs.Add(new Ssrc
                            {
                                Id = (uint)ssrc,
                                AttributesAndValues = new string[] { $"cname:{offerRtpParameters.Rtcp.Cname}" }
                            }); 
                        }

                        if (_planB)
                        {
                            _mediaObject.Ssrcs.Add(new Ssrc
                            {
                                Id = (uint)ssrc,
                                AttributesAndValues = new string[] { $"msid:{streamId ?? "-"} {trackId}" }
                            });
                        }

                        if (rtxSsrc.HasValue)
                        {
                            if (offerRtpParameters.Rtcp.Cname is not null)
                            {
                                _mediaObject.Ssrcs.Add(new Ssrc
                                {
                                    Id = (uint)rtxSsrc,
                                    AttributesAndValues = new string[] { $"cname:{offerRtpParameters.Rtcp.Cname}" }
                                });
                            }

                            if (_planB)
                            {
                                _mediaObject.Ssrcs.Add(new Ssrc
                                {
                                    Id = (uint)rtxSsrc,
                                    AttributesAndValues = new string[] { $"msid:{streamId ?? "-"} {trackId}" }
                                });
                            }

                            // Associate original and retransmission SSRCs.
                            _mediaObject.SsrcGroups.Add(new SsrcGroup
                            {
                                Semantics = "FID",
                                SsrcIds = new string[] { $"{ssrc} {rtxSsrc}"}
                            });
                        }

                        break;
                    }

                case MediaKind.Application:
                    {
                        // New spec.
                        if (!oldDataChannelSpec)
                        {
                            _mediaObject.Payloads = "webrtc-datachannel";
                            _mediaObject.SctpPort = sctpParameters.Port;
                            _mediaObject.MaxMessageSize = sctpParameters.MaxMessageSize;
                        }
                        // Old spec.
                        else
                        {
                            _mediaObject.Payloads = sctpParameters.Port.ToString();
                            _mediaObject.SctpMap = new()
                            {
                                App = "webrtc-datachannel",
						        SctpMapNumber =  sctpParameters.Port,
						        MaxMessageSize = sctpParameters.MaxMessageSize

                            };
                        }

                        break;
                    }
            }
        }

        void PlanBReceive(RtpParameters offerRtpParameters, string streamId, IMediaStream trackId)
        {
            var encoding = offerRtpParameters.Encodings[0];
            var ssrc = encoding.Ssrc;
            var rtxSsrc = (encoding.Rtx is not null && encoding.Rtx.Ssrc.HasValue) ?
                encoding.Rtx.Ssrc : null;
            var payloads = _mediaObject.Payloads.Split(' ');


            foreach (var codec in offerRtpParameters.Codecs)
            {
                if (payloads.Any(payload => payload.Contains(codec.PayloadType.ToString())))
                    continue;

                Rtpmap rtpmap = new()
                {
                    PayloadType = codec.PayloadType,
                    EncodingName = GetCodecName(codec),
                    ClockRate = codec.ClockRate,
                    Channels = codec.Channels > 1 ? codec.Channels : null
                };
                _mediaObject.Rtpmaps.Add(rtpmap);

                Fmtp fmtp = new()
                {
                    PayloadType = codec.PayloadType,
                    Value = string.Empty
                };
                //foreach (var key in codec.Parameters.Keys)
                //{
                //    if (!string.IsNullOrEmpty(fmtp.Value))
                //        fmtp.Value += ";";
                //    fmtp.Value += $"{key}={codec.Parameters[key]}";
                //}
                if (!string.IsNullOrEmpty(fmtp.Value))
                    _mediaObject.Fmtps.Add(fmtp);

                foreach (var fb in codec.RtcpFeedback)
                {
                    RtcpFb rtcpFb = new()
                    {
                        PayloadType = codec.PayloadType,
                        Type = fb.Type,
                        SubType = fb.Parameter
                    };
                    _mediaObject.RtcpFbs.Add(rtcpFb);
                }
            }

            _mediaObject.Payloads += string.Join(" ", offerRtpParameters.Codecs
                .Select(codec => codec.PayloadType.ToString())).Trim();


            if (offerRtpParameters.Rtcp.Cname is not null)
            {
                _mediaObject.Ssrcs.Add(new Ssrc
                {
                    Id = (uint)ssrc,
                    AttributesAndValues = new string[] { $"cname:{offerRtpParameters.Rtcp.Cname}" }
                });
            }

            _mediaObject.Ssrcs.Add(new Ssrc
            {
                Id = (uint)ssrc,
                AttributesAndValues = new string[] { $"msid:{streamId ?? "-"} {trackId}" }
            });

            if (rtxSsrc.HasValue)
            {
                if (offerRtpParameters.Rtcp.Cname is not null)
                {
                    _mediaObject.Ssrcs.Add(new Ssrc
                    {
                        Id = (uint)rtxSsrc,
                        AttributesAndValues = new string[] { $"cname:{offerRtpParameters.Rtcp.Cname}" }
                    });
                }

                _mediaObject.Ssrcs.Add(new Ssrc
                {
                    Id = (uint)rtxSsrc,
                    AttributesAndValues = new string[] { $"msid:{streamId ?? "-"} {trackId}" }
                });

                // Associate original and retransmission SSRCs.
                _mediaObject.SsrcGroups.Add(new SsrcGroup
                {
                    Semantics = "FID",
                    SsrcIds = new string[] { $"{ssrc} {rtxSsrc}" }
                });
            }
        }

        void PlanBStopReceiving(RtpParameters offerRtpParameters)
        {
            var encoding = offerRtpParameters.Encodings[0];
            var ssrc = encoding.Ssrc;
            var rtxSsrc = (encoding.Rtx is not null && encoding.Rtx.Ssrc.HasValue) ?
                encoding.Rtx.Ssrc : null;

            _mediaObject.Ssrcs.RemoveAll(s => s.Id != ssrc && s.Id != rtxSsrc);

            if (rtxSsrc is not null)
                _mediaObject.SsrcGroups.RemoveAll(g => g.SsrcIds.Any(id => id != $"{ssrc} {rtxSsrc}"));
        }

        protected override void SetDtlsRole(DtlsRole? dtlsRole)
        {
            _mediaObject.Setup = "actpass";
        }
    }
}
