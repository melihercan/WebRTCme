using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    class AnswerMediaSection : MediaSection
    {
        public AnswerMediaSection(
            IceParameters iceParameters,
            IceCandidate[] iceCandidates,
            DtlsParameters dtlsParameters,
            SctpParameters sctpParameters,
            PlainRtpParameters plainRtpParameters,
            bool planB,
            MediaObject offerMediaObject,
            RtpParameters offerRtpParameters,
            RtpParameters answerRtpParameters,
            ProducerCodecOptions codecOptions,
            bool extmapAllowMixed) : base(iceParameters, iceCandidates, dtlsParameters, planB)
        {
            _mediaObject.MediaDescription.Attributes.Mid = offerMediaObject.MediaDescription.Attributes.Mid;
            _mediaObject.MediaDescription.Media = offerMediaObject.MediaDescription.Media;
            _mediaObject.MediaDescription.Proto = offerMediaObject.MediaDescription.Proto;

            if (plainRtpParameters is null)
            {
                _mediaObject.MediaDescription.ConnectionData = new ConnectionData
                {
                    NetType = NetType.Internet,
                    AddrType = AddrType.Ip4,
                    ConnectionAddress = "127.0.0.1"
                };
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
                _mediaObject.MediaDescription.Port = plainRtpParameters.Port;
            }


            switch (offerMediaObject.MediaDescription.Media)
            {
                case MediaType.Audio:
                case MediaType.Video:
                    {
                        _mediaObject.Direction = Direction.RecvOnly;
                        _mediaObject.MediaDescription.Attributes.Rtpmaps = new List<Rtpmap>();
                        _mediaObject.MediaDescription.Attributes.RtcpFbs = new List<RtcpFb>();
                        _mediaObject.MediaDescription.Attributes.Fmtps = new List<Fmtp>();

                        foreach (var codec in answerRtpParameters.Codecs)
                        {
                            Rtpmap rtpmap = new()
                            {
                                PayloadType = codec.PayloadType,
                                EncodingName = GetCodecName(codec),
                                ClockRate = codec.ClockRate,
                                Channels = codec.Channels > 1 ? codec.Channels : null
                            };
                            _mediaObject.MediaDescription.Attributes.Rtpmaps.Add(rtpmap);

                            //var cp = codec.Parameters as CodecParameters;
                            //CodecParameters codecParameters = new()
                            //{
                            //    Stereo = cp.Stereo,
                            //    UseInBandFec = cp.UseInBandFec,
                            //    UsedTx = cp.UsedTx,
                            //    MaxPlaybackRate = cp.MaxPlaybackRate,
                            //    MaxAverageBitrate = cp.MaxAverageBitrate,
                            //    Ptime = cp.Ptime,
                            //    XGoogleStartBitrate = cp.XGoogleStartBitrate,
                            //    XGoogleMaxBitrate = cp.XGoogleMaxBitrate,
                            //    XGoogleMinBitrate = cp.XGoogleMinBitrate
                            //};

                            //if (codecOptions is not null)
                            //{
                            //    var offerCodec = offerRtpParameters.Codecs
                            //        .First(c => c.PayloadType == codec.PayloadType);
                            //    var ocp = offerCodec.Parameters as CodecParameters;
                            //    switch (codec.MimeType.ToLower())
                            //    {
                            //        case "audio/opus":
                            //            {
                            //                if (codecOptions.OpusStereo.HasValue)
                            //                {
                            //                    ocp.Stereo = 
                            //                        (bool)codecOptions.OpusStereo ? true : false;
                            //                    codecParameters.Stereo = 
                            //                        (bool)codecOptions.OpusStereo ? true : false;
                            //                }

                            //                if (codecOptions.OpusFec.HasValue)
                            //                {
                            //                    ocp.UseInBandFec = 
                            //                        (bool)codecOptions.OpusFec ? true : false;
                            //                    codecParameters.UseInBandFec = 
                            //                        (bool)codecOptions.OpusFec ? true :false;
                            //                }

                            //                if (codecOptions.OpusDtx.HasValue)
                            //                {
                            //                    ocp.UsedTx = 
                            //                        (bool)codecOptions.OpusDtx ? true : false;
                            //                    codecParameters.UsedTx = 
                            //                        (bool)codecOptions.OpusDtx ? true : false;
                            //                }

                            //                if (codecOptions.OpusMaxPlaybackRate.HasValue)
                            //                {
                            //                    codecParameters.MaxPlaybackRate = codecOptions.OpusMaxPlaybackRate;
                            //                }

                            //                if (codecOptions.OpusMaxAverageBitrate.HasValue)
                            //                {
                            //                    codecParameters.MaxAverageBitrate = codecOptions.OpusMaxAverageBitrate;
                            //                }

                            //                if (codecOptions.OpusPtime.HasValue)
                            //                {
                            //                    ocp.Ptime = codecOptions.OpusPtime;
                            //                    codecParameters.Ptime = codecOptions.OpusPtime;
                            //                }
                            //                break;
                            //            }

                            //        case "video/vp8":
                            //        case "video/vp9":
                            //        case "video/h264":
                            //        case "video/h265":
                            //            {
                            //                if (codecOptions.VideoGoogleStartBitrate is not null)
                            //                    codecParameters.XGoogleStartBitrate = 
                            //                        codecOptions.VideoGoogleStartBitrate;

                            //                if (codecOptions.VideoGoogleMaxBitrate is not null)
                            //                    codecParameters.XGoogleMaxBitrate = 
                            //                        codecOptions.VideoGoogleMaxBitrate;

                            //                if (codecOptions.VideoGoogleMinBitrate is not null)
                            //                    codecParameters.XGoogleMinBitrate = 
                            //                        codecOptions.VideoGoogleMinBitrate;

                            //                break;
                            //            }
                            //    }
                            //}

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

                        ((List<string>)_mediaObject.MediaDescription.Fmts).AddRange(answerRtpParameters.Codecs
                            .Select(codec => codec.PayloadType.ToString()));

                        _mediaObject.Extensions = new();
                        foreach (var headerExtension in answerRtpParameters.HeaderExtensions)
                        {
                            // Don't add a header extension if not present in the offer.
                            var found = offerMediaObject.Extensions
                                .Any(localExt => localExt.Uri == headerExtension.Uri);
                            if (!found)
                                continue;

                            var ext = new RtpHeaderExtensionParameters
                            {
                                Uri = headerExtension.Uri,
                                Number = headerExtension.Number
                            };
                            _mediaObject.Extensions.Add(ext);
                        }

                        // Allow both 1 byte and 2 bytes length header extensions.
                        if (extmapAllowMixed && offerMediaObject.ExtmapAllowMixed == "extmap-allow-mixed")
                            _mediaObject.ExtmapAllowMixed = "extmap-allow-mixed";

                        // Simulcast.
                        if (offerMediaObject.Simulcast is not null)
                        {
                            _mediaObject.Simulcast = new()
                            {
                                Dir1 = RidDirection.Recv,
                        	    List1 = offerMediaObject.Simulcast.List1
                            };

                            _mediaObject.MediaDescription.Attributes.Rids = new List<Rid>();
                            foreach (var rid in offerMediaObject.MediaDescription.Attributes.Rids)
                            {
                                if (rid.Direction != RidDirection.Send)
                                    continue;

                                _mediaObject.MediaDescription.Attributes.Rids.Add(new Rid
                                {
                                    Id = rid.Id,
                            	    Direction = RidDirection.Recv
                                });
                            }
                        }

                        // Simulcast (draft version 03).
                        else if (offerMediaObject.Simulcast03 is not null)
                        {
                            _mediaObject.Simulcast03 = new()
                            {
                                Value = offerMediaObject.Simulcast03.Value.Replace(RidDirection.Send.DisplayName(),
                                    RidDirection.Recv.DisplayName())
                            };

                            _mediaObject.MediaDescription.Attributes.Rids = new List<Rid>();
                            foreach (var rid in offerMediaObject.MediaDescription.Attributes.Rids)
                            {
                                if (rid.Direction != RidDirection.Send)
                                    continue;

                                _mediaObject.MediaDescription.Attributes.Rids.Add(new Rid
                                {
                                    Id = rid.Id,
                            		Direction = RidDirection.Recv
                                });
                            }
                    }

                        _mediaObject.MediaDescription.Attributes.RtcpMux = true;
                        _mediaObject.MediaDescription.Attributes.RtcpRsize = true;

                        if (_planB && _mediaObject.MediaDescription.Media == MediaType.Video)
                            _mediaObject.XGoogleFlag = "conference";
                        break;
                    }

                case MediaType.Application:
                    {
                        // New spec.
                        if (offerMediaObject.MediaDescription.Attributes.SctpPort.GetType() == typeof(int))
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
                        else if (offerMediaObject.SctpMap is not null)
                        {
                            _mediaObject.MediaDescription.Fmts = new List<string> { sctpParameters.Port.ToString() };
                            _mediaObject.SctpMap = new()
                            {
                                App = "webrtc-datachannel",
						        SctpMapNumber = sctpParameters.Port,
						        MaxMessageSize = sctpParameters.MaxMessageSize
                            };
                        }

                        break;
                    }
            }
        }

        public override void SetDtlsRole(DtlsRole? dtlsRole)
        {
            _mediaObject.MediaDescription.Attributes.Setup = new Setup
            {
                Role = dtlsRole switch
                {
                    DtlsRole.Client => SetupRole.Active,
                    DtlsRole.Server => SetupRole.Passive,
                    DtlsRole.Auto => SetupRole.ActPass,
                    _ => throw new NotImplementedException()
                }
            };
        }
    }
}
