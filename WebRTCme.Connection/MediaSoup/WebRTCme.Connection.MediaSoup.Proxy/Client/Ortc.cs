﻿using System;
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

        // Validates RtpCapabilities. It may modify given data by adding missing
        // fields with default values.
        // It throws if invalid.
        public void ValidateRtpCapabilities(RtpCapabilities caps)
        {
            // Codecs is optional. If unset, fill with an empty array.
            if (caps.Codecs is null)
                caps.Codecs = new RtpCodecCapability[] { };
            foreach (var codec in caps.Codecs)
                ValidateRtpCodecCapability(codec);

            // HeaderExtensions is optional. If unset, fill with an empty array.
            if (caps.HeaderExtensions is null)
                caps.HeaderExtensions = new RtpHeaderExtension[] { };
            foreach (var ext in caps.HeaderExtensions)
                ValidateRtpHeaderExtension(ext);
        }

        // Validates RtpCodecCapability. It may modify given data by adding missing
        // fields with default values.
        // It throws if invalid.
        public void ValidateRtpCodecCapability(RtpCodecCapability codec)
        {
            var mimeTypeRegex = new Regex("^(audio|video)/(.+)", RegexOptions.IgnoreCase);

            // mimeType is mandatory.
            if (codec.MimeType is null)
                throw new Exception("missing codec.mimeType");

            var mimeTypeMatch = mimeTypeRegex.Match(codec.MimeType);
            if (!mimeTypeMatch.Success)
                throw new Exception("invalid codec.mimeType");

            // Just override kind with media component of mimeType.
            codec.Kind =  (MediaKind)Enum.Parse(typeof(MediaKind), mimeTypeMatch.Value.ToLower(), true);

            // Channels is optional. If unset, set it to 1 (just if audio).
            if (codec.Kind ==  MediaKind.Audio)
            {
                if (!codec.Channels.HasValue)
                    codec.Channels = 1;
            }
            else
            {
                codec.Channels = null;
            }

            // Parameters is optional. If unset, set it to an empty object.
            if (codec.Parameters is null)
                codec.Parameters = new object { };

            // RtcpFeedback is optional. If unset, set it to an empty array.
            if (codec.RtcpFeedback is null)
                codec.RtcpFeedback = new RtcpFeedback[] { };
            foreach (var fb in codec.RtcpFeedback)
	            ValidateRtcpFeedback(fb);
        }

        // Validates RtcpFeedback. It may modify given data by adding missing
        // fields with default values.
        // It throws if invalid.
        public void ValidateRtcpFeedback(RtcpFeedback fb)
        {
            // Type is mandatory.
            if (fb.Type is null)
                throw new Exception("missing fb.type");

            // Parameter is optional. If unset set it to an empty string.
            if (fb.Parameter is null)
                fb.Parameter = string.Empty;

        }

        public void ValidateRtpHeaderExtension(RtpHeaderExtension ext)
        {
            // Kind is mandatory.
            if (ext.Kind != MediaKind.Audio && ext.Kind != MediaKind.Video)
                throw new Exception("invalid ext.kind");

            // Uri is mandatory.
            if (ext.Uri is null)
                throw new Exception("missing ext.uri");

            // PreferredEncrypt is optional. If unset set it to false.
            if (!ext.PreferredEncrypt.HasValue)
                ext.PreferredEncrypt = false;

            // Direction is optional. If unset set it to sendrecv.
            if (!ext.Direction.HasValue)
                ext.Direction = Direction.Sendonly;
        }

        public void ValidateRtpParameters(RtpParameters params_)
        {
            // Codecs is mandatory.
            if (params_.Codecs is null)
                throw new Exception("missing params_.codecs");
            foreach (var codec in params_.Codecs)
	        {
                ValidateRtpCodecParameters(codec);
            }

            // HeaderExtensions is optional. If unset, fill with an empty array.
            if (params_.HeaderExtensions is null)
		        params_.HeaderExtensions = new RtpHeaderExtensionParameters[] { };
            foreach (var ext in params_.HeaderExtensions)
                ValidateRtpHeaderExtensionParameters(ext);

            // Encodings is optional. If unset, fill with an empty array.
            if (params_.Encodings is null)
		        params_.Encodings =  new RtpEncodingParameters[] { };
            foreach (var encoding in params_.Encodings)
                ValidateRtpEncodingParameters(encoding);

            // Rtcp is optional. If unset, fill with an empty object.
            if (params_.Rtcp is null)
		        params_.Rtcp = new RtcpParameters();
            ValidateRtcpParameters(params_.Rtcp);
        }

        public void ValidateRtpCodecParameters(RtpCodecParameters codec)
        {
            var mimeTypeRegex = new Regex("^(audio|video)/(.+)", RegexOptions.IgnoreCase);

            // MimeType is mandatory.
            if (codec.MimeType is null)
                throw new Exception("missing codec.mimeType");
            var mimeTypeMatch = mimeTypeRegex.Match(codec.MimeType);
            if (!mimeTypeMatch.Success)
                throw new Exception("invalid codec.mimeType");

            var kind = (MediaKind)Enum.Parse(typeof(MediaKind), mimeTypeMatch.Value.ToLower(), true);

            // Channels is optional. If unset, set it to 1 (just if audio).
            if (kind == MediaKind.Audio)
            {
                if (!codec.Channels.HasValue)
                    codec.Channels = 1;
            }
            else
            {
                codec.Channels = null;
            }

            // RtcpFeedback is optional. If unset, set it to an empty array.
            if (codec.RtcpFeedback is null)
                codec.RtcpFeedback = new RtcpFeedback[] { };
            foreach (var fb in codec.RtcpFeedback)
                ValidateRtcpFeedback(fb);
        }

        public void ValidateRtpHeaderExtensionParameters(RtpHeaderExtensionParameters ext)
        {
            // Uri is mandatory.
            if (ext.Uri is null)
                throw new Exception("missing ext.uri");

            // Encrypt is optional. If unset set it to false.
            if (!ext.Encrypt.HasValue)
                ext.Encrypt = false;
        }

        public void ValidateRtpEncodingParameters(RtpEncodingParameters encoding)
        {
            if (encoding.Rtx is not null)
            {
                // RTX ssrc is mandatory if rtx is present.
                if (!encoding.Rtx.Ssrc.HasValue)
                    throw new Exception("missing encoding.rtx.ssrc");
            }

            // Dtx is optional. If unset set it to false.
            if (!encoding.Dtx.HasValue)
                encoding.Dtx = false;
        }

        public void ValidateRtcpParameters(RtcpParameters rtcp)
        {
            // ReducedSize is optional. If unset set it to true.
            if (!rtcp.ReducedSize.HasValue)
                rtcp.ReducedSize = true;
        }

        public void ValidateSctpCapabilities(SctpCapabilities caps)
        {
            // NumStreams is mandatory.
            if (caps.NumStreams is null)
                throw new Exception("missing caps.numStreams");

            ValidateNumSctpStreams(caps.NumStreams);
        }

        public void ValidateNumSctpStreams(NumSctpStreams numStreams)
        {
        }

        public void ValidateSctpParameters(SctpParameters params_)
        {

        }

        public void ValidateSctpStreamParameters(SctpStreamParameters params_)
        {
            // ordered is optional.
            var orderedGiven = false;

            if (params_.Ordered.HasValue)
		        orderedGiven = true;
            else
		        params_.Ordered = true;

            if (params_.MaxPacketLifeTime.HasValue && params_.MaxRetransmits.HasValue)
		        throw new Exception("cannot provide both maxPacketLifeTime and maxRetransmits");

            if (orderedGiven &&	(bool)params_.Ordered && 
                (params_.MaxPacketLifeTime.HasValue || params_.MaxRetransmits.HasValue))
                    throw new Exception("cannot be ordered with maxPacketLifeTime or maxRetransmits");
            else if (!orderedGiven && (params_.MaxPacketLifeTime.HasValue || params_.MaxRetransmits.HasValue))
		        params_.Ordered = false;
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
                    SendId = matchingLocalExt.PreferredId,
                    RecvId = remoteExt.PreferredId,
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
                    PreferredId = extendedExtensions.RecvId,
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

        public RtpCodecParameters[] ReduceCodecs(RtpCodecParameters[] codecs, RtpCodecCapability capCodec)
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

        public bool CanSend(MediaKind kind, ExtendedRtpCapabilities extendedRtpCapabilities)
        {
            return extendedRtpCapabilities.Codecs.Any(codec => codec.Kind == kind);
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
