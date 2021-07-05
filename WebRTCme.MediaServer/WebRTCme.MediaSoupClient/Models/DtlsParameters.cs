using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.MediaSoupClient.Enums;

namespace WebRTCme.MediaSoupClient.Models
{
    class DtlsParameters
    {
        public DtlsRole? DtlsRole { get; init; } 
        public DtlsFingerprint[] Fingerprints { get; init; }
    }
}
