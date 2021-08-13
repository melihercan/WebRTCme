using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Group
    {
        public const string Label = "group:";

        public GroupSemantics Semantics { get; set; }

        public string[] SemanticsExtensions { get; set; }

    }
}
