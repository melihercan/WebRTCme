using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware.MediaStreamProxies.Enums.MediaSoup;

namespace WebRTCme.Middleware.MediaStreamProxies.Models.MediaSoup
{
    class DtlsParameters
    {
        public DtlsRole? DtlsRole { get; init; } 
        public DtlsFingerprint[] Fingerprints { get; init; }
    }
}
