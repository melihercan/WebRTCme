using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class MediaObject
    {
        public IceUfrag IceUfrag { get; set; }
        public IcePwd IcePwd { get; set; }
        public Candidate[] Candidates { get; set; } 

    }
}
