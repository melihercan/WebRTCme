using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class DataConsumerRequestParameters
    {
        public string PeerId { get; init; } // null if bot
        public string DataProducerId { get; init; }
        public string Id { get; init; }
        public SctpStreamParameters SctpStreamParameters { get; init; }
        public string Label { get; init; }
        public string Protocol { get; init; }
        public Dictionary<string, object> AppData { get; init; }
    }
}
