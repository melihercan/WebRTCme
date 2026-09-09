using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class DtlsParameters
    {
        public DtlsRole? Role { get; set; } 
        public DtlsFingerprint[] Fingerprints { get; set; }
    }
}
