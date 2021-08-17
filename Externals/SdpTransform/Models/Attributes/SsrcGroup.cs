using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class SsrcGroup
    {
        public const string Label = "ssrc-group:";

        public string Semantics { get; set; }

        public string[] SsrcIds { get; set; }

    }
}
