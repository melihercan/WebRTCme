using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilme.SdpTransform;

namespace Utilme.SdpTransform
{
    public class AttributeParser
    {
        public static RtpMapAttribute[] ToRtpMapAttributes(IList<string>mediaDesriptionAttributes)
        {
            var rtpMaps = mediaDesriptionAttributes.Where(mda => mda.StartsWith("a=rtpmap"));

            return null;
        }
    }
}
