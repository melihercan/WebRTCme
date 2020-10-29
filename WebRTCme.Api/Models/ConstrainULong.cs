using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class ConstrainULong : ULongRange
    {
        public ulong? Single { get; set; }

        public ulong? Exact { get; set; }
        public ulong? Ideal { get; set; }
    }
}
