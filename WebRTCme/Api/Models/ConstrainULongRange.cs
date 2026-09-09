using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class ConstrainULongRange : ULongRange
    {
        public ulong? Exact { get; set; }
        public ulong? Ideal { get; set; }
    }
}
