using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class ProduceDataRequest
    {
        public string TransportId { get; init; }
        public SctpStreamParameters SctpStreamParameters { get; init; }
        public string Label { get; init; }
        public string Protocol { get; init; }
        public Dictionary<string, object> AppData { get; init; }
    }
}
