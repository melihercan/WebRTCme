using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    // a=ssrc:<ssrc-id> <attribute>
    // a=ssrc:<ssrc-id> <attribute>:<value>
    
    public class Ssrc
    {
        public const string Label = "ssrc:";

        public uint Id { get; set; }
        public string Attribute { get; set; }
        public string Value { get; set; }


    }
}
