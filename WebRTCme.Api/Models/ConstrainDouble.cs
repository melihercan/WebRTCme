using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class ConstrainDouble : DoubleRange
    {
        public double? Single { get; set; }

        public double? Exact { get; set; }
        public double? Ideal { get; set; }
    }
}
