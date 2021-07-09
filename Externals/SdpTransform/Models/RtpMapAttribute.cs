using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class RtpMapAttribute
    {
        public int PayloadType { get; set; }
        public string EncodingName { get; set; }
        public int ClockRate { get; set; }
        public string EncodingParameters { get; set; }
    }
}
