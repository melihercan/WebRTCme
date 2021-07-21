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


        public Direction Direction { get; set; }
        public int Port { get; set; }


    }
}
