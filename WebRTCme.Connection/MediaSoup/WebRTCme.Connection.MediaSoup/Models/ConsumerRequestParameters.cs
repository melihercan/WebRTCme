using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class ConsumerRequestParameters
    {
        public string PeerId { get; init; }
        public string ProducerId { get; init; }
        public string ConsumerId { get; init; }
        public MediaKind? Kind { get; init; }
        public RtpParameters RtpParameters { get; init; }
        public ConsumerType? Type { get; init; }
        // Settable because it is normalised after deserialisation: System.Text.Json leaves
        // JsonElements here that ToStringOrNumberOrBool has to replace, and doing that to an
        // init-only property meant editing the dictionary behind its owner's back.
        public Dictionary<string, object> AppData { get; set; }
        public bool ProducerPaused { get; init; }

    }
}
