using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCDataChannelInit
    {
        public bool? Ordered { get; set; }

        public ushort? MaxPacketLifeTime { get; set; }

        public ushort? MaxRetransmits { get; set; }

        public string Protocol { get; set; }

        public bool? Negotiated { get; set; }

        public short? Id { get; set; } 
    }
}
