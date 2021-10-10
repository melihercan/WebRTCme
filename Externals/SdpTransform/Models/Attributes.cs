using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Attributes
    {
        // Binary attributes.
        public bool? ExtmapAllowMixed { get; set; }
        public const string ExtmapAllowMixedLabel = "extmap-allow-mixed";

        public bool? IceLite { get; set; }
        public const string IceLiteLabel = "ice-lite";

        public bool? RtcpMux { get; set; }
        public const string RtcpMuxLabel = "rtcp-mux";

        public bool? RtcpRsize { get; set; }
        public const string RtcpRsizeLabel = "rtcp-rsize";


        public bool? SendRecv { get; set; }
        public const string SendRecvLabel = "sendrecv";

        public bool? SendOnly { get; set; }
        public const string SendOnlyLabel = "sendonly";

        public bool? RecvOnly { get; set; }
        public const string RecvOnlyLabel = "recvonly";

        public bool? EndOfCandidates { get; set; }
        public const string EndOfCandidatesLabel = "end-of-candidates";




        // Value attributes.
        public Group Group { get; set; }

        public MsidSemantic MsidSemantic { get; set; }

        public Mid Mid { get; set; }

        public Msid Msid { get; set; }

        public IceUfrag IceUfrag { get; set; }

        public IcePwd IcePwd { get; set; }

        public IceOptions IceOptions { get; set; }

        public Fingerprint Fingerprint { get; set; }

        public Rtcp Rtcp { get; set; }

        public Setup Setup { get; set; }

        public SctpPort SctpPort { get; set; }

        public MaxMessageSize MaxMessageSize { get; set; }

        public Simulcast Simulcast { get; set; }

        public IList<Candidate> Candidates { get; set; }
 
        public IList<Ssrc> Ssrcs { get; set; }

        public IList<SsrcGroup> SsrcGroups { get; set; }

        public IList<Rid> Rids { get; set; }

        public IList<Rtpmap> Rtpmaps { get; set; }

        public IList<Fmtp> Fmtps { get; set; }

        public IList<RtcpFb> RtcpFbs { get; set; }

        public IList<Extmap> Extmaps { get; set; }



    }
}
