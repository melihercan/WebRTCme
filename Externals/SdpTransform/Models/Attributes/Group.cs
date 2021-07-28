using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Group
    {
        public const string Name = "group:";

        public string Type { get; init; }
        public const string BundleType = "BUNDLE";

        public string[] Tokens { get; set; }

    }
}
