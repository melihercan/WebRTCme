using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class SctpMap
    {
        public string App { get; init; }
        public int SctpMapNumber { get; init; }
        public int MaxMessageSize { get; init; }
    }
}
