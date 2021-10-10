using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class ConsumerRequestParameters
    {
        public string PeerId { get; init; }
        public string ProducerId { get; init; }
        public string Id { get; init; }
        public MediaKind? Kind { get; init; }
        public RtpParameters RtpParameters { get; init; }
        public ConsumerType? Type { get; init; }
        public Dictionary<string, object> AppData { get; init; }
        public bool ProducerPaused { get; init; }

    }
}
