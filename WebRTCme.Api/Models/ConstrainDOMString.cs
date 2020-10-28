using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class ConstrainDOMString
    {
        public class Object
        {
            public string Single { get; set; }
            public string[] Array { get; set; }
        }

        public string Single { get; set; }
        public string[] Array { get; set; }

        Object Exact { get; set; } 
        Object Ideal { get; set; }
    }
}
