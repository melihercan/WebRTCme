﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    // a=extmap:<value>["/"<direction>] <URI> <extensionattributes>
    public class Extmap
    {
        public const string Label = "extmap:";

        public int Value { get; set; }
        public string Direction { get; set; }
        public string Uri { get; set; }
        public string ExtensionAttributes { get; set; }
    }
}
