using UtilmeSdpTransform;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    static class SdpCommonUtils
    {
        public static RtpCapabilities ExtractRtpCapabilities(Sdp sdp)
        {
            Dictionary<int, RtpCodecCapability> codecsDictionary = new();
            List<RtpHeaderExtension> headerExtensions = new();

            bool gotAudio = false;
            bool gotVideo = false;

            foreach (var m in sdp.MediaDescriptions)
            {
                var kind = m.Media;
                MediaKind mediaKind = MediaKind.Video;
                switch (kind)
                {
                    case "audio":
                        mediaKind = MediaKind.Audio;
                        if (gotAudio)
                            continue;
                        gotAudio = true;
                        break;
                    case "video":
                        mediaKind = MediaKind.Video;
                        if (gotVideo)
                            continue;
                        gotVideo = true;
                        break;
                    default:
                        continue;
                }

                //foreach (var rtp in m.)
                var rtpMapAttributes = AttributeParser.ToRtpMapAttributes(m);
                foreach (var rtp in rtpMapAttributes)
                {
                    var codec = new RtpCodecCapability 
                    {
                        Kind = mediaKind,
                        MimeType = $"{kind}/{rtp.EncodingName}",
                        PreferredPayloadType = rtp.PayloadType,
                        ClockRate = rtp.ClockRate,
                        Channels = rtp.EncodingParameters is null ? null : int.Parse(rtp.EncodingParameters) 
                    };
                    codecsDictionary.Add(codec.PreferredPayloadType, codec);
                }

            }


            return null;
        }
    }
}
