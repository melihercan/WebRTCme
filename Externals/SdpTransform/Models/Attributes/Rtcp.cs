using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Rtcp
    {
        public const string Label = "rtcp:";

        public int Port { get; set; }

        public NetType? NetType { get; set; }

        public AddrType? AddrType { get; set; }

        // Optional.
        public string ConnectionAddress { get; set; }

    }
}
