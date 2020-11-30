using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCDataChannelInit
    {
        bool? Ordered { get; set; } 

        ushort? MaxPacketLifeTime { get; set; }

        ushort? MaxRetransmits { get; set; }

        string Protocol { get; set; }

        bool? Negotiated { get; set; }

        short? Id { get; set; } 
    }
}
