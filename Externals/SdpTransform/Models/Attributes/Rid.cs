using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
        // a=rid:1 recv pt=97;max-width=1280;max-height=720
    public class Rid
    {
        public const string Label = "rid:";

        public string Id { get; init; }

        public RidDirection Direction { get; init; }

        public string[] FmtList { get; init; }          // format of each string: "pt=<value>"
        
        public string[] Restrictions { get; init; }     // format of each strings: "<restriction>=<value>"  



    }
}
