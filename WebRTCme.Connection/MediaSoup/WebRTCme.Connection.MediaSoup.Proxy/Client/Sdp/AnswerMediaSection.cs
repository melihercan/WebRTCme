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
            _mediaObject.Mid = offerMediaObject.Mid;
            _mediaObject.Kind = offerMediaObject.Kind;
            _mediaObject.Protocol = offerMediaObject.Protocol;

            if (plainRtpParameters is null)
            {
                _mediaObject.Connection = new ConnectionData
                {
                    Nettype = "IN",
                    AddrType = IpVersion.Ip4.DisplayName(),
                    ConnectionAddress = "127.0.0.1"
                };
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
                _mediaObject.Port = plainRtpParameters.Port;
            }


            switch (offerMediaObject.Kind)
            {
                case MediaKind.Audio:
                case MediaKind.Video:
                    {
                        _mediaObject.Direction = Direction.Recvonly;
                        _mediaObject.Rtpmaps = new();
                        _mediaObject.RtcpFbs = new();
                        _mediaObject.Fmtps = new();

                        foreach (var codec in answerRtpParameters.Codecs)
                        {
                            Rtpmap rtpmap = new()
                            {
                                PayloadType = codec.PayloadType,
                                EncodingName = GetCodecName(codec),
                                ClockRate = codec.ClockRate,
                                Channels = codec.Channels > 1 ? codec.Channels : null
                            };
                            _mediaObject.Rtpmaps.Add(rtpmap);

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

                        _mediaObject.Payloads += string.Join(" ", answerRtpParameters.Codecs
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

                            _mediaObject.Rids = new();
                            foreach (var rid in offerMediaObject.Rids)
                            {
                                if (rid.Direction != RidDirection.Send)
                                    continue;

                                _mediaObject.Rids.Add(new Rid
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

                            _mediaObject.Rids = new();
                            foreach (var rid in offerMediaObject.Rids)
                            {
                                if (rid.Direction != RidDirection.Send)
                                    continue;

                                _mediaObject.Rids.Add(new Rid
                                {
                                    Id = rid.Id,
                            		Direction = RidDirection.Recv
                                });
                            }
                    }

                    _mediaObject.BinaryAttributes.RtcpMux = true;
                        _mediaObject.BinaryAttributes.RtcpRsize = true;

                        if (_planB && _mediaObject.Kind == MediaKind.Video)
                            _mediaObject.XGoogleFlag = "conference";
                        break;
                    }

                case MediaKind.Application:
                    {
                        // New spec.
                        if (offerMediaObject.SctpPort.GetType() == typeof(int))
                        {
                            _mediaObject.Payloads = "webrtc-datachannel";
                            _mediaObject.SctpPort = sctpParameters.Port;
                            _mediaObject.MaxMessageSize = sctpParameters.MaxMessageSize;
                        }
                        // Old spec.
                        else if (offerMediaObject.SctpMap is not null)
                        {
                            _mediaObject.Payloads = sctpParameters.Port.ToString();
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
            switch (dtlsRole)
            {
                case DtlsRole.Client:
                    _mediaObject.Setup = "active";
                    break;
                case DtlsRole.Server:
                    _mediaObject.Setup = "passive";
                    break;
                case DtlsRole.Auto:
                    _mediaObject.Setup = "actpass";
                    break;
            }
        }
    }
}
