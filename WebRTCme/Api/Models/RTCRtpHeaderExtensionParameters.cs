using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpHeaderExtensionParameters
    {
        public string Uri { get; init; }

        public ushort Id { get; init; }

        public bool Encrypted { get; init; }
    }
}
