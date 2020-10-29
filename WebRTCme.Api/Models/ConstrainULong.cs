using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class ConstrainULong
    {
        public ulong? Single { get; set; }

        public ulong? Min { get; set; }
        public ulong? Max { get; set; }
        public ulong? Exact { get; set; }
        public ulong? Ideal { get; set; }
    }
}
