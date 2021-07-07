using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.ConnectionServer
{
    public class DtlsParameters
    {
        public DtlsRole? DtlsRole { get; init; } 
        public DtlsFingerprint[] Fingerprints { get; init; }
    }
}
