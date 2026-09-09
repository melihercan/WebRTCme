using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    public class MediaObject
    {
        //public Mid Mid { get; set; }
        //public Msid Msid { get; set; }
        //public IceUfrag IceUfrag { get; set; }
        //public IcePwd IcePwd { get; set; }
        //public IceOptions IceOptions { get; set; }
        //public Fingerprint Fingerprint { get; set; }
        //public List<Candidate> Candidates { get; set; }
        //public List<Rtpmap> Rtpmaps { get; set; }
        //public List<RtcpFb> RtcpFbs { get; set; }
        //public List<Fmtp> Fmtps { get; set; }
        //public List<Ssrc> Ssrcs { get; set; }
        //public List<SsrcGroup> SsrcGroups { get; set; }
        //public List<Rid> Rids { get; set; }
        //public BinaryAttributes BinaryAttributes { get; set; }


        //public ConnectionData Connection { get; set; }
        //public Direction Direction { get; set; }
        //public int Port { get; set; }

        //public MediaKind Kind { get; set; }

        //public string Protocol { get; set; }

        //public string Payloads { get; set; }    // in this format: "P1 P2 P3 ... Pn"

        public MediaDescription MediaDescription { get; set; }


        ////public Direction Direction { get; set; }


        ////public List<RtpHeaderExtensionParameters> Extensions { get; set; }

        ////public int SctpPort { get; set; }
        ////public int MaxMessageSize { get; set; }

        public SctpMap SctpMap { get; set; }

        //public string Setup { get; set; }

        public string XGoogleFlag { get; set; }

        ////public string ExtmapAllowMixed { get; set; }

        ////public Simulcast Simulcast { get; set; }
        public Simulcast03 Simulcast03 { get; set; }



    }
}
