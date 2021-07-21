using UtilmeSdpTransform;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup;
using Utilme.SdpTransform;
using System.Linq;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    static class CommonUtils
    {
        public static RtpCapabilities ExtractRtpCapabilities(Utilme.SdpTransform.Sdp sdp)
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
                    SetMediaDescriptorParametersObject(codec, fmtp.Value);
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

        static void SetMediaDescriptorParametersObject(RtpCodecCapability codec, string fmtpValue)
        {
            var tokens = fmtpValue.Split(';');

            if (codec.MimeType.Equals("audio/opus"))
            {
                OpusParameters opus = new();
                foreach (var token in tokens)
                {
                    var kvp = token.Split('=');
                    if (kvp[0].Equals("useinbandfec"))
                        opus.UseInbandFec = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("usedtx"))
                        opus.UsedTx = int.Parse(kvp[1]);
                }
                codec.Parameters = opus;
            }
            if (codec.MimeType.Equals("video/H264"))
            {
                H264Parameters h264 = new();
                foreach (var token in tokens)
                {
                    var kvp = token.Split('=');
                    if (kvp[0].Equals("packetization-mode"))
                        h264.PacketizationMode = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("profile-level-id"))
                        h264.ProfileLevelId = kvp[1];
                    else if (kvp[0].Equals("level-asymmetry-allowed"))
                        h264.LevelAsymmetryAllowed = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-start-bitrate"))
                        h264.XGoogleStartBitrate = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-max-bitrate"))
                        h264.XGoogleMaxBitrate = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-min-bitrate"))
                        h264.XGoogleMinBitrate = int.Parse(kvp[1]);
                }
                codec.Parameters = h264;
            }
            else if (codec.MimeType.Equals("video/VP8"))
            {
                VP8Parameters vp8 = new();
                foreach (var token in tokens)
                {
                    var kvp = token.Split('=');
                    if (kvp[0].Equals("profile-id"))
                        vp8.ProfileId = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-start-bitrate"))
                        vp8.XGoogleStartBitrate = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-max-bitrate"))
                        vp8.XGoogleMaxBitrate = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-min-bitrate"))
                        vp8.XGoogleMinBitrate = int.Parse(kvp[1]);
                }
                codec.Parameters = vp8;
            }
            else if (codec.MimeType.Equals("video/VP9"))
            {
                VP9Parameters vp9 = new();
                foreach (var token in tokens)
                {
                    var kvp = token.Split('=');
                    if (kvp[0].Equals("profile-id"))
                        vp9.ProfileId = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-start-bitrate"))
                        vp9.XGoogleStartBitrate = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-max-bitrate"))
                        vp9.XGoogleMaxBitrate = int.Parse(kvp[1]);
                    else if (kvp[0].Equals("x-google-min-bitrate"))
                        vp9.XGoogleMinBitrate = int.Parse(kvp[1]);
                }
                codec.Parameters = vp9;
            }
            else if (codec.MimeType.Equals("video/rtx"))
            {
                RtxParameters rtx = new();
                foreach (var token in tokens)
                {
                    var kvp = token.Split('=');
                    if (kvp[0].Equals("apt"))
                        rtx.Apt = int.Parse(kvp[1]);
                }
                codec.Parameters = rtx;
            }
            else
                codec.Parameters = null;

        }
    }
}
