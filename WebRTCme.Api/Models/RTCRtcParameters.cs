using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtcParameters
    {
        public string Cname { get; }

        public bool ReducedSize { get; }

        public RTCRtcParameters(string cname, bool reducedSize)
        {
            Cname = cname;
            ReducedSize = reducedSize;
        }

    }
}
