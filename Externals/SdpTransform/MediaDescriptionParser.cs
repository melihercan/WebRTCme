using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utilme.SdpTransform;

namespace Utilme.SdpTransform
{
    public class MediaDescriptionParser
    {
        // https://www.iana.org/assignments/sdp-parameters/sdp-parameters.xhtml#sdp-parameters-12
        public static Rtpmap[] ToRtpmapAttributes(MediaDescription mediaDescription)
        {
            var attributes = mediaDescription.AttributesOld
                .Where(a => a.StartsWith("rtpmap:"))
                .ToArray();

            List<Rtpmap> rtpmapAttributes = new();
            foreach (var a in attributes)
            {
                var tokens = a.Substring(7).Split(new[] { ' ', '/' }, 4);
                rtpmapAttributes.Add(new Rtpmap 
                { 
                    PayloadType = int.Parse(tokens[0]),
                    EncodingName = tokens[1],
                    ClockRate = int.Parse(tokens[2]),
                    Channels = tokens.Length == 4 ? int.Parse(tokens[3]) : null
                });
            }

            return rtpmapAttributes.ToArray();
        }

        public static Fmtp[] ToFmtpAttributes(MediaDescription mediaDescription)
        {
            var attributes = mediaDescription.AttributesOld
                .Where(a => a.StartsWith("fmtp:"))
                .ToArray();

            List<Fmtp> fmtpAttributes = new();
            foreach (var a in attributes)
            {
                var tokens = a.Substring(5).Split(new char[] { ' ' }, 2);
                fmtpAttributes.Add(new Fmtp 
                {
                    PayloadType = int.Parse(tokens[0]),
                    Value = tokens[1],
                });
            }

            return fmtpAttributes.ToArray();
        }

        public static RtcpFb[] ToRtcpFbAttributes(MediaDescription mediaDescription)
        {
            var attributes = mediaDescription.AttributesOld
                .Where(a => a.StartsWith("rtcp-fb:"))
                .ToArray();

            List<RtcpFb> rtcpFbAttributes = new();
            foreach (var a in attributes)
            {
                var tokens = a.Substring(8).Split(new char[] { ' ' }, 3);
                rtcpFbAttributes.Add(new RtcpFb 
                {
                    PayloadType = int.Parse(tokens[0]),
                    Type = tokens[1],
                    SubType = tokens.Length == 3 ? tokens[2] : null
                });
            }

            return rtcpFbAttributes.ToArray();
        }

        public static Extmap[] ToExtmapAttributes(MediaDescription mediaDescription)
        {
            var attributes = mediaDescription.AttributesOld
                .Where(a => a.StartsWith("extmap:"))
                .ToArray();

            List<Extmap> extmapAttributes = new();
            foreach (var a in attributes)
            {
                // Direction is optional and attached to Value with '/'.
                var tokens = a.Substring(7).Split(new char[] { ' ' }, 3);
                var subtokens = tokens[0].Split(new char[] { '/' }, 2);
                extmapAttributes.Add(new Extmap
                {
                    Value = int.Parse(subtokens[0]),
                    Direction = subtokens.Length == 2 ? subtokens[1] : null,
                    Uri = tokens[1],
                    ExtensionAttributes = tokens.Length == 3 ? tokens[2] : null
                });
            }

            return extmapAttributes.ToArray();
        }
    }
}
