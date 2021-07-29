using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class SsrcGroup
    {
        public const string Name = "ssrc-group";

        public string Semantics { get; init; }

        public string[] SsrcIds { get; init; }

    }
}
