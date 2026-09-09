using UtilmeSdpTransform;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup;
using Utilme.SdpTransform;
using System.Linq;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

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
                    case MediaType.Audio:
                        mediaKind = MediaKind.Audio;
                        if (gotAudio)
                            continue;
                        gotAudio = true;
                        break;
                    case MediaType.Video:
                        mediaKind = MediaKind.Video;
                        if (gotVideo)
                            continue;
                        gotVideo = true;
                        break;
                    default:
                        continue;
                }

                var rtpmapAttributes = m.Attributes.Rtpmaps;
                foreach (var rtpmap in rtpmapAttributes)
                {
                    var codec = new RtpCodecCapability 
                    {
                        MimeType = $"{kind.DisplayName()}/{rtpmap.EncodingName}",
                        Kind = mediaKind,
                        PreferredPayloadType = rtpmap.PayloadType,
                        ClockRate = rtpmap.ClockRate,
                        Channels = rtpmap.Channels 
                    };
                    codecsDictionary.Add(codec.PreferredPayloadType, codec);
                }

                var fmtpAttributes = m.Attributes.Fmtps;
                foreach (var fmtp in fmtpAttributes)
                {
                    if (!codecsDictionary.TryGetValue(fmtp.PayloadType, out var codec))
                        continue;
                    codec.Parameters = fmtp.ToDictionary();
                }

                var rtcpFbAttributes = m.Attributes.RtcpFbs;
                foreach (var rtcpFb in rtcpFbAttributes)
                {
                    if (!codecsDictionary.TryGetValue(rtcpFb.PayloadType, out var codec))
                        continue;
                    RtcpFeedback fb = new() 
                    {
                        Type = rtcpFb.Type,
                        Parameter = rtcpFb.SubType
                    };
                    codec.RtcpFeedback ??= new RtcpFeedback[] { };
                    var rtcpFbList = codec.RtcpFeedback.ToList();
                    rtcpFbList.Add(fb);
                    codec.RtcpFeedback = rtcpFbList.ToArray();
                }
                

                var extmapAttributes = m.Attributes.Extmaps;
                foreach (var extmap in extmapAttributes)
                {
                    RtpHeaderExtension headerExtension = new()
                    {
                        Kind = mediaKind,
                        Uri = extmap.Uri.ToString(),
                        PreferredId = extmap.Value
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

#if false
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
#endif

        internal static DtlsParameters ExtractDtlsParameters(Utilme.SdpTransform.Sdp sdp)
        {
            MediaObject mediaObject = new()
            {
                MediaDescription = sdp.MediaDescriptions
                    .SingleOrDefault(md => md.Attributes.IceUfrag is not null &&  md.Port != 0)
            };
            if (mediaObject is null)
                throw new Exception("No active media section found");

            var fingerprint = mediaObject.MediaDescription.Attributes.Fingerprint ?? sdp.Attributes.Fingerprint;
            DtlsRole? role = mediaObject.MediaDescription.Attributes.Setup.Role switch
            {
                SetupRole.Active => DtlsRole.Client,
                SetupRole.Passive => DtlsRole.Server,
                SetupRole.ActPass => DtlsRole.Auto,
                _ => null
            };

            return new DtlsParameters 
            {
                Role = role,
                Fingerprints = new DtlsFingerprint[] 
                { 
                    new DtlsFingerprint
                    {
                        Algorithm = fingerprint.HashFunction.DisplayName(),
                        Value = BitConverter.ToString(fingerprint.HashValue).Replace("-",":")
                    }
                }  
	        };
        }

        //public static MediaDescription MediaObjectToSdpMediaDescription(MediaObject mediaObject)
        //{
            //var attributes = new List<string>();

            //if (mediaObject.Attributes.Mid is not null) 
            //    attributes.Add(mediaObject.Attributes.Mid.ToText());
            //if (mediaObject.Attributes.Msid is not null)
            //    attributes.Add(mediaObject.Attributes.Msid?.ToText());
            //if (mediaObject.Attributes.IceUfrag is not null)
            //    attributes.Add(mediaObject.Attributes.IceUfrag?.ToText());
            //if (mediaObject.Attributes.IcePwd is not null)
            //    attributes.Add(mediaObject.Attributes.IcePwd?.ToText());
            //if (mediaObject.Attributes.IceOptions is not null)
            //    attributes.Add(mediaObject.Attributes.IceOptions?.ToText());
            //if (mediaObject.Attributes.Fingerprint is not null)
            //    attributes.Add(mediaObject.Attributes.Fingerprint?.ToText());
            //if (mediaObject.Attributes.Candidates is not null)
            //    attributes.AddRange(mediaObject.Attributes.Candidates.Select(i => i.ToText()).ToList());
            //if (mediaObject.Attributes.Rtpmaps is not null)
            //    attributes.AddRange(mediaObject.Attributes.Rtpmaps.Select(i => i.ToText()).ToList());
            //if (mediaObject.Attributes.RtcpFbs is not null)
            //    attributes.AddRange(mediaObject.Attributes.RtcpFbs.Select(i => i.ToText()).ToList());
            //if (mediaObject.Attributes.Fmtps is not null)
            //    attributes.AddRange(mediaObject.Attributes.Fmtps.Select(i => i.ToText()).ToList());
            //if (mediaObject.Attributes.Ssrcs is not null)
            //    attributes.AddRange(mediaObject.Attributes.Ssrcs.Select(i => i.ToText()).ToList());
            //if (mediaObject.Attributes.SsrcGroups is not null)
            //    attributes.AddRange(mediaObject.Attributes.SsrcGroups.Select(i => i.ToText()).ToList());
            //if (mediaObject.Attributes.Rids is not null)
            //    attributes.AddRange(mediaObject.Attributes.Rids.Select(i => i.ToText()).ToList());

            //// MediaDescription attributes should not contain CRLF.
            //attributes = attributes.Select(a => a.Replace(Utilme.SdpTransform.Sdp.CRLF, string.Empty)).ToList();

            //return new MediaDescription
            //{
            //    Media = mediaObject.Media,
            //    Port = mediaObject.Port,
            //    Proto = mediaObject.Protocol,
            //    Fmts = mediaObject.Payloads.Split(' ').ToList(),
            //    Information = mediaObject.Kind.DisplayName(),
            //    ConnectionData = mediaObject.Connection,
            //    //Bandwiths = ???
            //    //EncriptionKey = ???? mediaObject.Fingerprint is not null ? 
            //    //    new EncriptionKey
            //    //    {
            //    //        Method = mediaObject.Fingerprint.HashFunction.DisplayName(),
            //    //        Value = Encoding.UTF8.GetString(mediaObject.Fingerprint.HashValue)
            //    //    } : null,
            //    AttributesOld = attributes
            //};
        //    throw new NotImplementedException();
        //}

        //public static MediaObject SdpMediaDescriptionToMediaObject(MediaDescription mediaDescription)
        //{
            //return new MediaObject
            //{
            //    Mid = mediaDescription.AttributesOld
            //        .SingleOrDefault(a => a.StartsWith(Mid.Label))?.ToMid(),
            //    Msid = mediaDescription.AttributesOld
            //        .SingleOrDefault(a => a.StartsWith(Msid.Label))?.ToMsid(),
            //    IceUfrag = mediaDescription.AttributesOld
            //        .SingleOrDefault(a => a.StartsWith(IceUfrag.Label))?.ToIceUfrag(),
            //    IcePwd = mediaDescription.AttributesOld
            //        .SingleOrDefault(a => a.StartsWith(IcePwd.Label))?.ToIcePwd(),
            //    IceOptions = mediaDescription.AttributesOld
            //            .SingleOrDefault(a => a.StartsWith(IceOptions.Label))?.ToIceOptions(),
            //    Fingerprint = mediaDescription.AttributesOld
            //            .SingleOrDefault(a => a.StartsWith(Fingerprint.Label))?.ToFingerprint(),
            //    Candidates = mediaDescription.AttributesOld
            //        .Where(a => a.StartsWith(Candidate.Label))
            //        .Select(c => c.ToCandidate())
            //        .ToList(),
            //    Rtpmaps = mediaDescription.AttributesOld
            //        .Where(a => a.StartsWith(Rtpmap.Label))
            //        .Select(r => r.ToRtpmap())
            //        .ToList(),
            //    RtcpFbs = mediaDescription.AttributesOld
            //        .Where(a => a.StartsWith(RtcpFb.Label))
            //        .Select(r => r.ToRtcpFb())
            //        .ToList(),
            //    Fmtps = mediaDescription.AttributesOld
            //        .Where(a => a.StartsWith(Fmtp.Label))
            //        .Select(r => r.ToFmtp())
            //        .ToList(),
            //    Ssrcs = mediaDescription.AttributesOld
            //        .Where(a => a.StartsWith(Ssrc.Label))
            //        .Select(r => r.ToSsrc())
            //        .ToList(),
            //    SsrcGroups = mediaDescription.AttributesOld
            //        .Where(a => a.StartsWith(SsrcGroup.Label))
            //        .Select(r => r.ToSsrcGroup())
            //        .ToList(),
            //    Rids = mediaDescription.AttributesOld
            //        .Where(a => a.StartsWith(Rid.Label))
            //        .Select(r => r.ToRid())
            //        .ToList()
            //};
        //    throw new NotImplementedException();

        //}

        public static string GetCname(MediaObject offerMediaObject)
        {
            var ssrcCnameLine = (offerMediaObject.MediaDescription.Attributes.Ssrcs ?? new List<Ssrc>())
                .FirstOrDefault(line => line .Attribute == "cname");
            if (ssrcCnameLine is null)
                return string.Empty;

            return ssrcCnameLine.Value;
        }

        public static void ApplyCodecParameters(RtpParameters offerRtpParameters, MediaObject answerMediaObject)
        {
            foreach (var codec in offerRtpParameters.Codecs)
	        {
                var mimeType = codec.MimeType.ToLower();

                // Avoid parsing codec parameters for unhandled codecs.
                if (mimeType != "audio/opus")
                    continue;

                var rtpmap = (answerMediaObject.MediaDescription.Attributes.Rtpmaps ?? new List<Rtpmap>())
                    .FirstOrDefault(r => r.PayloadType == codec.PayloadType);
                if (rtpmap is null)
                    continue;

                var fmtp = (answerMediaObject.MediaDescription.Attributes.Fmtps ?? new List<Fmtp>())
                    .FirstOrDefault(f => f.PayloadType == codec.PayloadType);
                if (fmtp is null)
                {
                    fmtp = new Fmtp { PayloadType = codec.PayloadType, Value = string.Empty };
                    answerMediaObject.MediaDescription.Attributes.Fmtps.Add(fmtp);
                }

                var parameters = fmtp.Value.Split(';').ToList();
                switch (mimeType)
                {
                    case "audio/opus":
                        {
                            var spropStereo = (int)(codec.Parameters.ContainsKey("sprop - stereo")
                                ? codec.Parameters["sprop - stereo"] : -1);
                            if (spropStereo != -1)
                            {
                                var stereo = spropStereo == 1 ? 1 : 0;
                                parameters.Add($"stereo={stereo}");
                            }
                            break;
                        }
                }

                // Write the codec fmtp.config back.
                fmtp.Value = string.Join(";", parameters.ToArray());
            }
        }
    }
}
