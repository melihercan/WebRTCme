using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.Proxy.Codecs;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    class Ortc
    {
        public void ValidateRtpCapabilities(RtpCapabilities caps)
        {

        }

        public ExtendedRtpCapabilities GetExtendedRtpCapabilites(RtpCapabilities localCaps, RtpCapabilities remoteCaps)
        {
            List<ExtendedRtpCodecCapability> codecs = new();

            // Match media codecs and keep the order preferred by remoteCaps.
            foreach (var remoteCodec in remoteCaps.Codecs)
            {
                if (IsRtxCodec(remoteCodec))
                    continue;

                var matchingLocalCodec = localCaps.Codecs.FirstOrDefault(
                    localCodec => MatchCodecs(localCodec, remoteCodec, strict: true, modify: true));
                if (matchingLocalCodec is null)
                    continue;
                codecs.Add(new ExtendedRtpCodecCapability 
                {
                    Kind = matchingLocalCodec.Kind,
                    MimeType = matchingLocalCodec.MimeType,
                    ClockRate = matchingLocalCodec.ClockRate,
                    Channels = matchingLocalCodec.Channels,
                    LocalPayloadType = matchingLocalCodec.PreferredPayloadType,
                    RemotePayloadType = remoteCodec.PreferredPayloadType,
                    LocalParameters = matchingLocalCodec.Parameters,
                    RemoteParameters = remoteCodec.Parameters,
                    RtcpFeedback = ReduceRtcpFeedback(matchingLocalCodec, remoteCodec)
                });
            }

            // Match RTX codecs.
            foreach (var extendedCodec in codecs)
            {
                var matchingLocalRtxCodec = localCaps.Codecs.FirstOrDefault(localCodec =>
                    IsRtxCodec(localCodec) &&
                    (bool)((RtxParameters)localCodec.Parameters).Apt?.Equals(extendedCodec.LocalRtxPayloadType));
                var matchingRemoteRtxCodec = remoteCaps.Codecs.FirstOrDefault(remoteCodec =>
                    IsRtxCodec(remoteCodec) &&
                    (bool)((RtxParameters)remoteCodec.Parameters).Apt?.Equals(extendedCodec.RemoteRtxPayloadType));
                if (matchingLocalRtxCodec is not null && matchingRemoteRtxCodec is not null)
                {
                    extendedCodec.LocalRtxPayloadType = matchingLocalRtxCodec.PreferredPayloadType;
                    extendedCodec.RemotePayloadType = matchingRemoteRtxCodec.PreferredPayloadType;
                }
            }

            List<ExtendedRtpHeaderExtensions> headerExtensions = new();

            // Match header extensions.
            foreach (var remoteExt in remoteCaps.HeaderExtensions)
            {
                var matchingLocalExt = localCaps.HeaderExtensions?.FirstOrDefault(localExt => 
                    MatchHeaderExtensions(localExt, remoteExt));
                if (matchingLocalExt is null)
                    continue;

                Direction direction = remoteExt.Direction switch
                {
                    Direction.Sendrecv => Direction.Sendrecv,
                    Direction.Recvonly => Direction.Sendonly,
                    Direction.Sendonly => Direction.Recvonly,
                    Direction.Inactive => Direction.Inactive,
                    _ => throw new NotImplementedException()
                };
                headerExtensions.Add(new ExtendedRtpHeaderExtensions 
                {
                    Kind = remoteExt.Kind,
                    Uri = remoteExt.Uri,
                    SendId = matchingLocalExt.PreferedId,
                    RecvId = remoteExt.PreferedId,
                    PreferredEncrypt = matchingLocalExt.PreferredEncrypt,
                    Direction = direction
                });
            }

            return new ExtendedRtpCapabilities 
            {
                Codecs = codecs.ToArray(),
                HeaderExtensions = headerExtensions.ToArray()
            };
        }

        bool MatchHeaderExtensions(RtpHeaderExtension aExt, RtpHeaderExtension bExt)
        {
            if (aExt.Kind != bExt.Kind)
                return false;
            if (!aExt.Uri.Equals(bExt.Uri))
                return false;
            return true;
        }

        bool IsRtxCodec(RtpCodecCapability codec) =>
            new Regex(@"(?i).+/rtx$").IsMatch(codec.MimeType);

        bool MatchCodecs(RtpCodecCapability aCodec, RtpCodecCapability bCodec, bool strict = false, 
            bool modify = false)
        {
            var aMimeType = aCodec.MimeType.ToLower();
            var bMimeType = bCodec.MimeType.ToLower();

            if (aMimeType != bMimeType)
                return false;

            if (aCodec.ClockRate != bCodec.ClockRate)
                return false;

            if (aCodec.Channels != bCodec.Channels)
                return false;

            switch (aMimeType)
            {
                case "video/h264":
                    var aH264CodecParameters = (H264Parameters)aCodec.Parameters;
                    var bH264CodecParameters = (H264Parameters)bCodec.Parameters;
                    
                    if (aH264CodecParameters.PacketizationMode != bH264CodecParameters.PacketizationMode)
                        return false;

                    if (strict)
                    {
                        if (!H264.IsSameProfile(aH264CodecParameters, bH264CodecParameters))
                            return false;

                        string selectedProfileLevelId;
                        try
                        {
                            selectedProfileLevelId = H264.GenerateProfileLevelIdForAnswer(
                                aH264CodecParameters, bH264CodecParameters);
                        }
                        catch
                        {
                            return false;
                        }
                         if (modify)
                        {
                            if (selectedProfileLevelId is not null)
                            {
                                aH264CodecParameters.ProfileLevelId = selectedProfileLevelId;
                                bH264CodecParameters.ProfileLevelId = selectedProfileLevelId;
                            }
                        }

                    }
                    break;

                case "video/vp9":
                    if (strict)
                    {
                        var aVp9CodecParameters = (VP9Parameters)aCodec.Parameters;
                        var bVp9CodecParameters = (VP9Parameters)bCodec.Parameters;

                        if (aVp9CodecParameters != bVp9CodecParameters)
                            return false;
                    }
                    break;
            }

            return true;
        }

        string GetCodecCapabilityParameterValue(RtpCodecCapability codec, string key)
        {
            string value = string.Empty;
            var tokens = new string[] { };
            
            if (codec.Parameters.GetType() == typeof(string))
            {
                tokens = ((string)codec.Parameters).Split(';');
            }
            else if (codec.Parameters.GetType() == typeof(JsonElement))
            {
                var parametersJson = ((JsonElement)codec.Parameters).GetRawText();
            }
            else
                throw new NotSupportedException($"Type {codec.Parameters.GetType()} is not supported");

            return value;
        }

        RtcpFeedback[] ReduceRtcpFeedback(RtpCodecCapability codecA, RtpCodecCapability codecB)
        {
            List<RtcpFeedback> reducedRtcpFeedback = new();

            foreach(var aFb in codecA.RtcpFeedback)
            {
                var matchingBFb = codecA.RtcpFeedback.FirstOrDefault(
                    bFb => bFb.Type.Equals(aFb.Type) &&
                    bFb.Parameter is not null && aFb.Parameter is not null || bFb.Parameter.Equals(aFb.Parameter));
                if (matchingBFb is not null)
                    reducedRtcpFeedback.Add(matchingBFb);
            }

            return reducedRtcpFeedback.ToArray();
        }
    }
}
