using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Rtpmap
    {
        public const string Label = "rtpmap:";
        public int PayloadType { get; set; }
        public string EncodingName { get; set; }
        public int ClockRate { get; set; }
        public int? Channels { get; set; }
    }
}
