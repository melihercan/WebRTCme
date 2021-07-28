using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Fingerprint
    {
        public const string Name = "fingerprint:";

        public HashFunction HashFunction { get; init; }
        
        public byte[] HashValue { get; init; }
    }
}
