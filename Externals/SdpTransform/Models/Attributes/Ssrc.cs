using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    // a=ssrc:<ssrc-id> <attribute>
    // a=ssrc:<ssrc-id> <attribute>:<value>

    
    public class Ssrc
    {
        public const string Name = "ssrc:";

        public uint Id { get; init; }
        public string[] AttributesAndValues { get; init; }


    }
}
