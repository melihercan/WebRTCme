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

        public RtpCapabilities GetExtendedRtpCapabilites(RtpCapabilities localCaps, RtpCapabilities remoteCaps)
        {
            RtpCapabilities extendedRtpCapabilities = new();

            foreach (var remoteCodec in remoteCaps.Codecs)
            {
                if (IsRtxCodec(remoteCodec))
                    continue;

                var matchingLocalCodec = localCaps.Codecs.FirstOrDefault(
                    localCodec => MatchCodecs(localCodec, remoteCodec, strict: true, modify: true));
                if (matchingLocalCodec is null)
                    continue;


            }


            throw new NotImplementedException();
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
    }
}
