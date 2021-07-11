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

                //foreach (var rtp in m.)

            }


            return null;
        }
    }
}
