using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    // a=simulcast:send r0;r1;r2
    public class Simulcast
    {
        public const string Label = "simulcast:";

        public RidDirection Direction { get; init; }

        public string[] IdList { get; init; }


    }
}
