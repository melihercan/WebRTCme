using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class ConstrainDoubleRange : DoubleRange
    {
        public double? Exact { get; set; }
        public double? Ideal { get; set; }
    }
}
