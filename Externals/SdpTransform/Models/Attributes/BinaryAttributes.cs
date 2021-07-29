using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class BinaryAttributes
    {
        public const string IceLiteName = "ice-lite";
        public bool? IceLite { get; set; }

        public const string RtcpMuxName = "rtcp-mux";
        public bool? RtcpMux { get; set; }

        public const string RtcpRsizeName = "rtcp-rsize";
        public bool? RtcpRsize { get; set; }



    }
}
