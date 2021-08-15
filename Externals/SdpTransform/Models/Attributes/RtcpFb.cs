using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class RtcpFb
    {
        public const string Label = "rtcp-fb:";
        public int PayloadType { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }

    }
}
