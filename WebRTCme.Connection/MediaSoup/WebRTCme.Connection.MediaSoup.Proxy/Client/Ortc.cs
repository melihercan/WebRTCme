using System;
using System.Collections.Generic;
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
                    var aCodecParameters = (H264Parameters)aCodec.Parameters;
                    var bCodecParameters = (H264Parameters)bCodec.Parameters;
                    
                    if (aCodecParameters.PacketizationMode != bCodecParameters.PacketizationMode)
                        return false;

                    if (strict)
                    {
                        if (!H264.IsSameProfile(aCodecParameters, bCodecParameters))
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
