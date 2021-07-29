using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    public class MediaObject
    {
        public Mid Mid { get; set; }
        public IceUfrag IceUfrag { get; set; }
        public IcePwd IcePwd { get; set; }
        public IceOptions IceOptions { get; set; }
        public Candidate[] Candidates { get; set; }
        public ConnectionData Connection { get; set; }
        public Rtpmap[] Rtpmap { get; set; }
        public RtcpFb[] RtcpFb { get; set; }
        public Fmtp[] Fmtp { get; set; }
        public Msid Msid { get; set; }

        public BinaryAttributes BinaryAttributes { get; set; } = new();

        public Direction Direction { get; set; }
        public int Port { get; set; }

        public MediaKind Kind { get; set; }

        public string Protocol { get; set; }

        public string Payloads { get; set; }

        public RtpHeaderExtensionParameters[] Extensions { get; set; }




    }
}
