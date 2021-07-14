using UtilmeSdpTransform;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup;
using Utilme.SdpTransform;
using System.Linq;

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

                var rtpmapAttributes = MediaDescriptionParser.ToRtpmapAttributes(m);
                foreach (var rtpmap in rtpmapAttributes)
                {
                    var codec = new RtpCodecCapability 
                    {
                        Kind = mediaKind,
                        MimeType = $"{kind}/{rtpmap.EncodingName}",
                        PreferredPayloadType = rtpmap.PayloadType,
                        ClockRate = rtpmap.ClockRate,
                        Channels = rtpmap.Channels 
                    };
                    codecsDictionary.Add(codec.PreferredPayloadType, codec);
                }

                var fmtpAttributes = MediaDescriptionParser.ToFmtpAttributes(m);
                foreach (var fmtp in fmtpAttributes)
                {
                    if (!codecsDictionary.TryGetValue(fmtp.PayloadType, out var codec))
                        continue;
                    codec.Parameters = fmtp.Value;
                }

                var rtcpFbAttributes = MediaDescriptionParser.ToRtcpFbAttributes(m);
                foreach (var rtcpFb in rtcpFbAttributes)
                {
                    if (!codecsDictionary.TryGetValue(rtcpFb.PayloadType, out var codec))
                        continue;
                    RtcpFeedback feedback = new() 
                    {
                        Type = rtcpFb.Type,
                        Parameter = rtcpFb.SubType
                    };
                    codec.RtcpFeedback = new RtcpFeedback[] { feedback };
                }

                var extmapAttributes = MediaDescriptionParser.ToExtmapAttributes(m);
                foreach (var extmap in extmapAttributes)
                {
                    RtpHeaderExtension headerExtension = new()
                    {
                        Kind = mediaKind,
                        Uri = extmap.Uri,
                        PreferedId = extmap.Value
                    };
                    headerExtensions.Add(headerExtension);
                }
            }

            RtpCapabilities rtpCapabilities = new()
            {
                Codecs = codecsDictionary.Values.ToArray(),
                HeaderExtensions = headerExtensions.ToArray()
            };
            return rtpCapabilities;
        }
    }
}
