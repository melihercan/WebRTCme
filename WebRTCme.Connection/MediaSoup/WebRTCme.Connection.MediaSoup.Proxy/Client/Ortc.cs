using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.Proxy.Codecs;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Ortc
    {

        const string RTP_PROBATOR_MID = "probator";
        const int RTP_PROBATOR_SSRC = 1234;
        const int RTP_PROBATOR_CODEC_PAYLOAD_TYPE = 127;

        public void ValidateRtpCapabilities(RtpCapabilities caps)
        {

        }

        public void ValidateSctpCapabilities(SctpCapabilities sctpCapabilities)
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
                    LocalParameters = (CodecParameters)matchingLocalCodec.Parameters,
                    RemoteParameters = (CodecParameters)remoteCodec.Parameters,
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


        public bool CanSend(MediaKind kind, ExtendedRtpCapabilities extendedRtpCapabilities)
        {
            return extendedRtpCapabilities.Codecs.Any(codec => codec.Kind == kind);
        }

        public RtpCapabilities GetRecvRtpCapabilities(ExtendedRtpCapabilities extendedRtpCapabilities)
        {
            List<RtpCodecCapability> codecs = new();
            foreach (var extendedCodec in extendedRtpCapabilities.Codecs)
            {
                RtpCodecCapability codec = new() 
                { 
                    Kind = extendedCodec.Kind,
                    MimeType = extendedCodec.MimeType,
                    PreferredPayloadType = extendedCodec.RemotePayloadType,
                    ClockRate = extendedCodec.ClockRate,
                    Channels = extendedCodec.Channels,
                    Parameters = extendedCodec.LocalParameters,
                    RtcpFeedback = extendedCodec.RtcpFeedback
                };
                codecs.Add(codec);

                if (extendedCodec.RemoteRtxPayloadType is null)
                    continue;

                RtpCodecCapability rtxCodec = new()
                {
                    Kind = extendedCodec.Kind,
                    MimeType = $"{extendedCodec.Kind}/rtx",
                    PreferredPayloadType = extendedCodec.RemotePayloadType,
                    ClockRate = extendedCodec.ClockRate,
                    Parameters = new RtxParameters
                    {
                        Apt = extendedCodec.RemotePayloadType
                    },
                    RtcpFeedback = new RtcpFeedback[] { }
                };
                codecs.Add(rtxCodec);
            }

            List<RtpHeaderExtension> headerExtensions = new();
            foreach (var extendedExtensions in extendedRtpCapabilities.HeaderExtensions)
            {
                if (extendedExtensions.Direction != Direction.Sendrecv
                    && extendedExtensions.Direction != Direction.Recvonly)
                    continue;

                RtpHeaderExtension ext = new() 
                { 
                    Kind = extendedExtensions.Kind,
                    Uri = extendedExtensions.Uri,
                    PreferedId = extendedExtensions.RecvId,
                    PreferredEncrypt = extendedExtensions.PreferredEncrypt,
                    Direction = extendedExtensions.Direction
                };
                headerExtensions.Add(ext);
            }

            return new RtpCapabilities
            {
                Codecs = codecs.ToArray(),
                HeaderExtensions = headerExtensions.ToArray()
            };
        }

        public void ValidateRtpParameters(RtpParameters rtpParameters)
        {
            throw new NotImplementedException();
        }

        public RtpParameters GetSendingRtpParameters(MediaKind kind, ExtendedRtpCapabilities extendedRtpCapabilities)
        {

            List<RtpCodecParameters> codecs = new();
            foreach (var extendedCodec in extendedRtpCapabilities.Codecs)
            {
                if (extendedCodec.Kind != kind)
                    continue;

                RtpCodecParameters codec = new()
                {
                    MimeType = extendedCodec.MimeType,
                    PayloadType = extendedCodec.LocalPayloadType,
                    ClockRate = extendedCodec.ClockRate,
                    Channels = extendedCodec.Channels,
                    Parameters = extendedCodec.LocalParameters,
                    RtcpFeedback = extendedCodec.RtcpFeedback
                };
                codecs.Add(codec);

                if (extendedCodec.LocalRtxPayloadType.HasValue)
                {
                    RtpCodecParameters rtxCodec = new()
                    {
                        MimeType = $"{extendedCodec.Kind.DisplayName()}/rtx",
                        PayloadType = (int)extendedCodec.LocalRtxPayloadType,
                        ClockRate = extendedCodec.ClockRate,
                        Parameters = extendedCodec.LocalParameters,
                        RtcpFeedback = new RtcpFeedback[] { }
                    };
                    codecs.Add(rtxCodec);
                }
            }

            List<RtpHeaderExtensionParameters> headerExtensions = new();
            foreach (var extendedExtendion in extendedRtpCapabilities.HeaderExtensions)
            {
                // Ignore RTP extensions of a different kind and those not valid for sending.
                if ((extendedExtendion.Kind != kind) ||
                    (extendedExtendion.Direction != Direction.Sendrecv && 
                    extendedExtendion.Direction != Direction.Sendonly))
                    continue;

                RtpHeaderExtensionParameters ext = new()
                {
                    Uri = extendedExtendion.Uri,
                    Number = extendedExtendion.SendId,
                    Encrypt = (bool)extendedExtendion.PreferredEncrypt,
                    Parameters = new()
                };
                headerExtensions.Add(ext);
            }

            return new RtpParameters 
            {
                Codecs = codecs.ToArray(),
                HeaderExtensions = headerExtensions.ToArray(),
                Encodings = new RtpEncodingParameters[] { },
                Rtcp = new()
            };
        }

        public bool CanReceive(RtpParameters rtpParameters, ExtendedRtpCapabilities extendedRtpCapabilities)
        {
            // This may throw.
            ValidateRtpParameters(rtpParameters);

            if (rtpParameters.Codecs.Length == 0)
                return false;

            var firstMediaCodec = rtpParameters.Codecs[0];
            return extendedRtpCapabilities.Codecs
                .Any(codec => codec.RemotePayloadType == firstMediaCodec.PayloadType);
        }

        public  RtpCodecParameters[] ReduceCodecs(RtpCodecParameters[] codecs, RtpCodecCapability capCodec)
        {
            List<RtpCodecParameters> filteredCodecs = new();

            // If no capability codec is given, take the first one (and RTX).
            if (capCodec is not null)
            {
                filteredCodecs.Add(codecs[0]);

                if (IsRtxCodec(codecs[1]))
                    filteredCodecs.Add(codecs[1]);
            }
            // Otherwise look for a compatible set of codecs.
            else
            {
                for (var idx = 0; idx < codecs.Length; ++idx)
                {
                    if (MatchCodecs(codecs[idx], capCodec))
                    {
                        filteredCodecs.Add(codecs[idx]);

                        if (IsRtxCodec(codecs[idx + 1]))
                            filteredCodecs.Add(codecs[idx + 1]);
                        break;
                    }
                }

                if (filteredCodecs.Count() == 0)
                    throw new Exception("No matching codec found");
            }

            return filteredCodecs.ToArray();
        }

        public RtpParameters GenerateProbatorRtpParameters(RtpParameters inRtpParameters)
        {
            // Clone given reference video RTP parameters.
            var videoRtpParameters = Utils.Clone(inRtpParameters, new RtpParameters());

            // This may throw.
            ValidateRtpParameters(videoRtpParameters);

            RtpCodecParameters codec = videoRtpParameters.Codecs[0];
            codec.PayloadType = RTP_PROBATOR_CODEC_PAYLOAD_TYPE;

            var rtpParameters = new RtpParameters
            {
                Mid = new Mid { Id = RTP_PROBATOR_MID },
		        Codecs = new RtpCodecParameters[] { codec },
		        HeaderExtensions = videoRtpParameters.HeaderExtensions,
		        Encodings = new RtpEncodingParameters[] { new RtpEncodingParameters { Ssrc = RTP_PROBATOR_SSRC } },
                Rtcp = new RtcpParameters { Cname = RTP_PROBATOR_MID } 
            };

            return rtpParameters;
        }

        public RtpParameters GetSendingRemoteRtpParameters(MediaKind kind, 
            ExtendedRtpCapabilities extendedRtpCapabilities)
        {

            List<RtpCodecParameters> codecs = new();
            foreach (var extendedCodec in extendedRtpCapabilities.Codecs)
            {
                if (extendedCodec.Kind != kind)
                    continue;

                RtpCodecParameters codec = new()
                {
                    MimeType = extendedCodec.MimeType,
                    PayloadType = extendedCodec.LocalPayloadType,
                    ClockRate = extendedCodec.ClockRate,
                    Channels = extendedCodec.Channels,
                    Parameters = extendedCodec.RemoteParameters,
                    RtcpFeedback = extendedCodec.RtcpFeedback
                };
                codecs.Add(codec);

                if (extendedCodec.LocalRtxPayloadType.HasValue)
                {
                    RtpCodecParameters rtxCodec = new()
                    {
                        MimeType = $"{extendedCodec.Kind.DisplayName()}/rtx",
                        PayloadType = (int)extendedCodec.LocalRtxPayloadType,
                        ClockRate = extendedCodec.ClockRate,
                        Parameters = extendedCodec.LocalParameters,
                        RtcpFeedback = new RtcpFeedback[] { }
                    };
                    codecs.Add(rtxCodec);
                }
            }

            List<RtpHeaderExtensionParameters> headerExtensions = new();
            foreach (var extendedExtendion in extendedRtpCapabilities.HeaderExtensions)
            {
                // Ignore RTP extensions of a different kind and those not valid for sending.
                if ((extendedExtendion.Kind != kind) ||
                    (extendedExtendion.Direction != Direction.Sendrecv &&
                    extendedExtendion.Direction != Direction.Sendonly))
                    continue;

                RtpHeaderExtensionParameters ext = new()
                {
                    Uri = extendedExtendion.Uri,
                    Number = extendedExtendion.SendId,
                    Encrypt = (bool)extendedExtendion.PreferredEncrypt,
                    Parameters = new()
                };
                headerExtensions.Add(ext);
            }

            // Reduce codecs' RTCP feedback. Use Transport-CC if available, REMB otherwise.
            if (headerExtensions.Any(ext => 
                ext.Uri == "http://www.ietf.org/id/draft-holmer-rmcat-transport-wide-cc-extensions-01"))
            {
                foreach (var codec in codecs)
		        {
                    codec.RtcpFeedback = codec.RtcpFeedback
                        .Where(fb => fb.Type != "goog-remb").ToArray();
                }
            }
            else if (headerExtensions.Any(ext => 
                    ext.Uri == "http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time"))
            {
                foreach (var codec in codecs)
		        {
                    codec.RtcpFeedback = codec.RtcpFeedback
                        .Where(fb => fb.Type != "transport-cc").ToArray();
                }
            }
            else
            {
                foreach (var codec in codecs)
		        {
                    codec.RtcpFeedback = codec.RtcpFeedback
                        .Where(fb => fb.Type != "transport-cc" && fb.Type != "goog-remb").ToArray();
                }
            }

            return new RtpParameters
            {
                Codecs = codecs.ToArray(),
                HeaderExtensions = headerExtensions.ToArray(),
                Encodings = new RtpEncodingParameters[] { },
                Rtcp = new()
            };
        }

        internal void ValidateSctpStreamParameters(SctpStreamParameters sctpStreamParameters)
        {
            throw new NotImplementedException();
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

        bool IsRtxCodec(RtpCodecParameters codec) =>
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

        bool MatchCodecs(RtpCodecParameters aCodec, RtpCodecParameters bCodec, bool strict = false,
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


        bool MatchCodecs(RtpCodecParameters aCodec, RtpCodecCapability bCodec, bool strict = false,
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
