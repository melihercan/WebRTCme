using SDPLib;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.ConnectionServer;

namespace WebRTCme.MediaSoupClient.Api
{
    static class SdpCommonUtils
    {
        public static RtpCapabilities ExtractRtpCapabilities(SDP sdp)
        {
            Dictionary<int, RtpCodecCapability> codecsDictionary = new();
            List<RtpHeaderExtension> headerExtensions = new();

            bool gotAudio = false;
            bool gotVideo = false;

            foreach (var m in sdp.MediaDescriptions)
            {
                var kind = m.Media;
                switch (kind)
                {
                    case "audio":
                        if (gotAudio)
                            continue;
                        gotAudio = true;
                        break;
                    case "video":
                        if (gotVideo)
                            continue;
                        gotVideo = true;
                        break;
                    default:
                        continue;
                }
            }

            //for (var )

            return null;
        }
    }
}
