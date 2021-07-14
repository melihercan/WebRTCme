using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utilme.SdpTransform;

namespace Utilme.SdpTransform
{
    public class AttributeParser
    {
        public static RtpMapAttribute[] ToRtpMapAttributes(MediaDescription mediaDesription)
        {
            var attributes = mediaDesription.Attributes
                .Where(a => a.StartsWith("rtpmap:"))
                .ToArray();

            List<RtpMapAttribute> rtpMapAttributes = new();
            foreach(var a in attributes)
            {
                var tokens = a.Substring(7).Split(new[] { ' ', '/' }, 4);
                rtpMapAttributes.Add(new RtpMapAttribute 
                { 
                    PayloadType = int.Parse(tokens[0]),
                    EncodingName = tokens[1],
                    ClockRate = int.Parse(tokens[2]),
                    EncodingParameters = tokens.Length == 4 ? tokens[3] : null,
                });

            }

            return rtpMapAttributes.ToArray();

        }
    }
}
