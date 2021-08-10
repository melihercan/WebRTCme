using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Attributes
    {
        public bool? IceLite { get; set; }
        public const string IceLiteLabel = "ice-lite";

        public bool? RtcpMux { get; set; }
        public const string RtcpMuxLabel = "rtcp-mux";

        public bool? RtcpRsize { get; set; }
        public const string RtcpRsizeLabel = "rtcp-rsize";

        public bool? EndOfCandidates { get; set; }
        public const string EndOfCandidatesLabel = "end-of-candidates";

    }
}
