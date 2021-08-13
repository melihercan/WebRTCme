using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class TimeZone
    {
        public DateTime AdjustmentTime { get; set; }
        public TimeSpan Offset { get; set; }
    }
}
