using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class DtlsParameters
    {
        public DtlsRole? DtlsRole { get; set; } 
        public DtlsFingerprint[] Fingerprints { get; set; }
    }
}
