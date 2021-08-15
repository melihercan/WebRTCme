using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Fingerprint
    {
        public const string Label = "fingerprint:";

        public HashFunction HashFunction { get; set; }

        // Each byte in upper-case hex, separated by colons.
        public byte[] HashValue { get; set; }
    }
}
