using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

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
            _mediaObject.MediaDescription.Attributes.Mid = mid;
            _mediaObject.MediaDescription.Media = kind.ToSdp();

            if (plainRtpParameters is null)
            {
                _mediaObject.MediaDescription.ConnectionData = new ConnectionData 
                {
                    NetType = NetType.Internet,
                    AddrType = AddrType.Ip4,
                    ConnectionAddress = "127.0.0.1"
                };
                if (sctpParameters is null)
                    _mediaObject.MediaDescription.Proto = "UDP/TLS/RTP/SAVPF";
                else
                    _mediaObject.MediaDescription.Proto = "UDP/DTLS/SCTP";
                _mediaObject.MediaDescription.Port = 7;
            }
            else
            {
                _mediaObject.MediaDescription.ConnectionData = new ConnectionData
                {
                    NetType = NetType.Internet,
                    AddrType = plainRtpParameters.IpVersion,
                    ConnectionAddress = plainRtpParameters.Ip
                };
                _mediaObject.MediaDescription.Proto = "RTP/AVP";
                _mediaObject.MediaDescription.Port = plainRtpParameters.Port;
            }

            switch (kind)
            {
                case MediaKind.Audio:
                case MediaKind.Video:
                    {
                        _mediaObject.MediaDescription.Attributes.SendOnly = true;
                        _mediaObject.MediaDescription.Attributes.Rtpmaps = new List<Rtpmap>();
                        _mediaObject.MediaDescription.Attributes.RtcpFbs = new List<RtcpFb>();
                        _mediaObject.MediaDescription.Attributes.Fmtps = new List<Fmtp>();

                        if (!_planB)
                            _mediaObject.MediaDescription.Attributes.Msid = new() 
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
                            _mediaObject.MediaDescription.Attributes.Rtpmaps.Add(rtpmap);

                            Fmtp fmtp = codec.Parameters.ToFmtp(codec.PayloadType);
                            if (!string.IsNullOrEmpty(fmtp.Value))
                                _mediaObject.MediaDescription.Attributes.Fmtps.Add(fmtp);

                            foreach (var fb in codec.RtcpFeedback)
                            {
                                RtcpFb rtcpFb = new() 
                                {
                                    PayloadType = codec.PayloadType,
                                    Type = fb.Type,
                                    SubType = fb.Parameter
                                };
                                _mediaObject.MediaDescription.Attributes.RtcpFbs.Add(rtcpFb);
                            }
                        }

                        _mediaObject.MediaDescription.Fmts ??= new List<string>();
                        ((List<string>)_mediaObject.MediaDescription.Fmts).AddRange(offerRtpParameters.Codecs
                            .Select(codec => codec.PayloadType.ToString()));


                        _mediaObject.MediaDescription.Attributes.Extmaps = new List<Extmap>();
                        foreach (var headerExtension in offerRtpParameters.HeaderExtensions)
                        {
                            Extmap ext = new()
                            {
                                Uri = new Uri(headerExtension.Uri),
                                Value = headerExtension.Id
                            };
                            _mediaObject.MediaDescription.Attributes.Extmaps.Add(ext);
                        }

                        _mediaObject.MediaDescription.Attributes.RtcpMux = true;
                        _mediaObject.MediaDescription.Attributes.RtcpRsize = true;

                        var encoding = offerRtpParameters.Encodings[0];
                        var ssrc = encoding.Ssrc;
                        var rtxSsrc = (encoding.Rtx is not null && encoding.Rtx.Ssrc.HasValue) ? 
                            encoding.Rtx.Ssrc : null;

                        _mediaObject.MediaDescription.Attributes.Ssrcs = new List<Ssrc>();
                        _mediaObject.MediaDescription.Attributes.SsrcGroups = new List<SsrcGroup>();
                        if (offerRtpParameters.Rtcp.Cname is not null)
                        {
                            _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                            {
                                Id = (uint)ssrc,
                                Attribute = "cname",
                                Value = offerRtpParameters.Rtcp.Cname
                            }); 
                        }

                        if (_planB)
                        {
                            _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                            {
                                Id = (uint)ssrc,
                                Attribute = "msid",
                                Value = $"{streamId ?? "-"} {trackId}"
                            });
                        }

                        if (rtxSsrc.HasValue)
                        {
                            if (offerRtpParameters.Rtcp.Cname is not null)
                            {
                                _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                                {
                                    Id = (uint)rtxSsrc,
                                    Attribute = "cname",
                                    Value = offerRtpParameters.Rtcp.Cname
                                });
                            }

                            if (_planB)
                            {
                                _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                                {
                                    Id = (uint)rtxSsrc,
                                    Attribute = "msid",
                                    Value = $"{streamId ?? "-"} {trackId}"
                                });
                            }

                            // Associate original and retransmission SSRCs.
                            _mediaObject.MediaDescription.Attributes.SsrcGroups.Add(new SsrcGroup
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
                            _mediaObject.MediaDescription.Fmts = new List<string> { "webrtc-datachannel" };
                            _mediaObject.MediaDescription.Attributes.SctpPort = new SctpPort
                            {
                                Port = sctpParameters.Port
                            };
                            _mediaObject.MediaDescription.Attributes.MaxMessageSize = new MaxMessageSize
                            {
                                Size = sctpParameters.MaxMessageSize
                            };
                        }
                        // Old spec.
                        else
                        {
                            _mediaObject.MediaDescription.Fmts = new List<string> { sctpParameters.Port.ToString() };
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
            var payloads = _mediaObject.MediaDescription.Fmts.ToArray();


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
                _mediaObject.MediaDescription.Attributes.Rtpmaps.Add(rtpmap);

                Fmtp fmtp = codec.Parameters.ToFmtp(codec.PayloadType);
                if (!string.IsNullOrEmpty(fmtp.Value))
                    _mediaObject.MediaDescription.Attributes.Fmtps.Add(fmtp);

                foreach (var fb in codec.RtcpFeedback)
                {
                    RtcpFb rtcpFb = new()
                    {
                        PayloadType = codec.PayloadType,
                        Type = fb.Type,
                        SubType = fb.Parameter
                    };
                    _mediaObject.MediaDescription.Attributes.RtcpFbs.Add(rtcpFb);
                }
            }

            ((List<string>)_mediaObject.MediaDescription.Fmts).AddRange(offerRtpParameters.Codecs
                .Select(codec => codec.PayloadType.ToString()));


            if (offerRtpParameters.Rtcp.Cname is not null)
            {
                _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                {
                    Id = (uint)ssrc,
                    Attribute = "cname",
                    Value = offerRtpParameters.Rtcp.Cname
                });
            }

            _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
            {
                Id = (uint)ssrc,
                Attribute = "msid",
                Value = $"{streamId ?? "-"} {trackId}"
            });

            if (rtxSsrc.HasValue)
            {
                if (offerRtpParameters.Rtcp.Cname is not null)
                {
                    _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                    {
                        Id = (uint)rtxSsrc,
                        Attribute = "cname",
                        Value = offerRtpParameters.Rtcp.Cname
                    });
                }

                _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                {
                    Id = (uint)rtxSsrc,
                    Attribute = "msid",
                    Value = $"{streamId ?? "-"} {trackId}"
                });

                // Associate original and retransmission SSRCs.
                _mediaObject.MediaDescription.Attributes.SsrcGroups.Add(new SsrcGroup
                {
                    Semantics = "FID",
                    SsrcIds = new string[] { $"{ssrc} {rtxSsrc}" }
                });
            }
        }

        public void PlanBReceive(RtpParameters offerRtpParameters, string streamId, string trackId)
        {
            var encoding = offerRtpParameters.Encodings[0];
            var ssrc = encoding.Ssrc;
            var rtxSsrc = (encoding.Rtx is not null && encoding.Rtx.Ssrc.HasValue)
                ? encoding.Rtx.Ssrc
                : null;
            var payloads = _mediaObject.MediaDescription.Fmts.ToArray();

            foreach (var codec in offerRtpParameters.Codecs)
		    {
                if (payloads.Contains(codec.PayloadType.ToString()))
                {
                    continue;
                }

                Rtpmap rtpmap = new()
                {
                    PayloadType = codec.PayloadType,
                    EncodingName = GetCodecName(codec),
                    ClockRate = codec.ClockRate,
                    Channels = codec.Channels > 1 ? codec.Channels : null
                };
                _mediaObject.MediaDescription.Attributes.Rtpmaps.Add(rtpmap);

                Fmtp fmtp = codec.Parameters.ToFmtp(codec.PayloadType);
                
                if (!string.IsNullOrEmpty(fmtp.Value))
                    _mediaObject.MediaDescription.Attributes.Fmtps.Add(fmtp);

                foreach (var fb in codec.RtcpFeedback)
			    {
                    _mediaObject.MediaDescription.Attributes.RtcpFbs.Add(new RtcpFb
                    {
                        PayloadType = codec.PayloadType,
						Type = fb.Type,
						SubType = fb.Parameter
                    });
                }
            }

            ((List<string>)_mediaObject.MediaDescription.Fmts).AddRange(offerRtpParameters.Codecs
                .Where(codec => !_mediaObject.MediaDescription.Fmts.Contains(codec.PayloadType.ToString()))
                .Select(codec => codec.PayloadType.ToString())
                .ToArray());

		    if (offerRtpParameters.Rtcp.Cname is not null)
            {
                _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc 
                {
                    Id = (uint)ssrc,
					Attribute = "cname",
                    Value = offerRtpParameters.Rtcp.Cname
                });
            }

		    _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
            {
                Id = (uint)ssrc,
				Attribute = "msid",
				Value = $"{ streamId ?? "-"} { trackId}"
			});

		    if (rtxSsrc is not null)
            {
                if (offerRtpParameters.Rtcp.Cname is not null)
                {
                    _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                    {
                        Id = (uint)rtxSsrc,
						Attribute = "cname",
						Value = offerRtpParameters.Rtcp.Cname
                    });
                }

                _mediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
                {
                    Id = (uint)rtxSsrc,
			        Attribute = "msid",
				    Value = $"{ streamId ?? "-"} {trackId}"
			    });

                // Associate original and retransmission SSRCs.
                _mediaObject.MediaDescription.Attributes.SsrcGroups.Add(new SsrcGroup
                {
                    Semantics = "FID",
				    SsrcIds = new string[] {$"{ ssrc }${rtxSsrc}" }
			    });
            }
        }

        void PlanBStopReceiving(RtpParameters offerRtpParameters)
        {
            var encoding = offerRtpParameters.Encodings[0];
            var ssrc = encoding.Ssrc;
            var rtxSsrc = (encoding.Rtx is not null && encoding.Rtx.Ssrc.HasValue) ?
                encoding.Rtx.Ssrc : null;

            ((List<Ssrc>)_mediaObject.MediaDescription.Attributes.Ssrcs).RemoveAll(s => s.Id != ssrc && s.Id != rtxSsrc);

            if (rtxSsrc is not null)
                ((List<SsrcGroup>)_mediaObject.MediaDescription.Attributes.SsrcGroups).RemoveAll(g => g.SsrcIds.Any(id => id != $"{ssrc} {rtxSsrc}"));
        }

        public override void SetDtlsRole(DtlsRole? dtlsRole)
        {
            _mediaObject.MediaDescription.Attributes.Setup = new Setup
            {
                Role = SetupRole.ActPass
            };
        }
    }
}
