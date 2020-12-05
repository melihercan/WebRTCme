using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpHeaderExtensionParameters
    {
        public string Uri { get; }

        public ushort Id { get; }

        public bool Encrypted { get; }
    }
}
