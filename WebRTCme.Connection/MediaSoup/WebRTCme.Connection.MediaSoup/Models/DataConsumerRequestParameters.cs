using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class DataConsumerRequestParameters
    {
        public string PeerId { get; init; } // null if bot
        public string DataProducerId { get; init; }
        public string DataConsumerId { get; init; }
        public SctpStreamParameters SctpStreamParameters { get; init; }
        public string Label { get; init; }
        public string Protocol { get; init; }
        // Settable because it is normalised after deserialisation: System.Text.Json leaves
        // JsonElements here that ToStringOrNumberOrBool has to replace, and doing that to an
        // init-only property meant editing the dictionary behind its owner's back.
        public Dictionary<string, object> AppData { get; set; }
    }
}
