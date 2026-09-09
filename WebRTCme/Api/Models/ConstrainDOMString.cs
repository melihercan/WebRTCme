using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class ConstrainDOMString
    {
        public string Value { get; set; }
        public string[] Array { get; set; }


        public ConstrainDOMStringUnion Exact { get; set; } 
        public ConstrainDOMStringUnion Ideal { get; set; }
    }
}
