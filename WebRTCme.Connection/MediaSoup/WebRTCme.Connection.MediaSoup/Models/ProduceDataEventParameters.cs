using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class ProduceDataEventParameters
    {
        public SctpStreamParameters SctpStreamParameters { get; init; }
        public string Label { get; init; }
        public string Protocol { get; init; }
        public Dictionary<string, object> AppData { get; init; }

    }
}
