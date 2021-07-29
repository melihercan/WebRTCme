using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilme.SdpTransform;

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

            if (plainRtpParameters is not null)
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

       //                     if (codecOptions)
       //                     {
       //                         const {
       //                             opusStereo,
							//opusFec,
							//opusDtx,
							//opusMaxPlaybackRate,
							//opusMaxAverageBitrate,
							//opusPtime,
							//videoGoogleStartBitrate,
							//videoGoogleMaxBitrate,
							//videoGoogleMinBitrate
        
       //                 } = codecOptions;

       //                         const offerCodec = offerRtpParameters!.codecs
       //                             .find((c: RtpCodecParameters) => (
       //                                 c.payloadType === codec.payloadType
       //                             ));

       //                         switch (codec.mimeType.toLowerCase())
       //                         {
       //                             case 'audio/opus':
       //                                 {
       //                                     if (opusStereo !== undefined)
       //                                     {
       //                                         offerCodec!.parameters['sprop-stereo'] = opusStereo ? 1 : 0;
       //                                         codecParameters.stereo = opusStereo ? 1 : 0;
       //                                     }

       //                                     if (opusFec !== undefined)
       //                                     {
       //                                         offerCodec!.parameters.useinbandfec = opusFec ? 1 : 0;
       //                                         codecParameters.useinbandfec = opusFec ? 1 : 0;
       //                                     }

       //                                     if (opusDtx !== undefined)
       //                                     {
       //                                         offerCodec!.parameters.usedtx = opusDtx ? 1 : 0;
       //                                         codecParameters.usedtx = opusDtx ? 1 : 0;
       //                                     }

       //                                     if (opusMaxPlaybackRate !== undefined)
       //                                     {
       //                                         codecParameters.maxplaybackrate = opusMaxPlaybackRate;
       //                                     }

       //                                     if (opusMaxAverageBitrate !== undefined)
       //                                     {
       //                                         codecParameters.maxaveragebitrate = opusMaxAverageBitrate;
       //                                     }

       //                                     if (opusPtime !== undefined)
       //                                     {
       //                                         offerCodec!.parameters.ptime = opusPtime;
       //                                         codecParameters.ptime = opusPtime;
       //                                     }

       //                                     break;
       //                                 }

       //                             case 'video/vp8':
       //                             case 'video/vp9':
       //                             case 'video/h264':
       //                             case 'video/h265':
       //                                 {
       //                                     if (videoGoogleStartBitrate !== undefined)
       //                                         codecParameters['x-google-start-bitrate'] = videoGoogleStartBitrate;

       //                                     if (videoGoogleMaxBitrate !== undefined)
       //                                         codecParameters['x-google-max-bitrate'] = videoGoogleMaxBitrate;

       //                                     if (videoGoogleMinBitrate !== undefined)
       //                                         codecParameters['x-google-min-bitrate'] = videoGoogleMinBitrate;

       //                                     break;
       //                                 }
       //                         }
       //                     }





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
     //                   if (
     //                       extmapAllowMixed &&
     //                       offerMediaObject.extmapAllowMixed === 'extmap-allow-mixed'
     //                   )
     //                   {
     //                       this._mediaObject.extmapAllowMixed = 'extmap-allow-mixed';
     //                   }

     //                   // Simulcast.
     //                   if (offerMediaObject.simulcast)
     //                   {
     //                       this._mediaObject.simulcast =
        
     //               {
     //                       dir1: 'recv',
					//	list1: offerMediaObject.simulcast.list1

     //               };

     //                       this._mediaObject.rids = [];

     //                       for (const rid of offerMediaObject.rids || [])
					//{
     //                           if (rid.direction !== 'send')
     //                               continue;

     //                           this._mediaObject.rids.push(

     //                       {
     //                           id: rid.id,
					//			direction: 'recv'

     //                       });
     //                       }
     //                   }
     //                   // Simulcast (draft version 03).
     //                   else if (offerMediaObject.simulcast_03)
     //                   {
     //                       // eslint-disable-next-line camelcase
     //                       this._mediaObject.simulcast_03 =
        
     //               {
     //                       value: offerMediaObject.simulcast_03.value.replace(/ send / g, 'recv')

     //               };

     //                       this._mediaObject.rids = [];

     //                       for (const rid of offerMediaObject.rids || [])
					//{
     //                           if (rid.direction !== 'send')
     //                               continue;

     //                           this._mediaObject.rids.push(

     //                       {
     //                           id: rid.id,
					//			direction: 'recv'

     //                       });
     //                       }
     //                   }

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

        protected override void SetDtlsRole(DtlsRole? dtlsRole)
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
