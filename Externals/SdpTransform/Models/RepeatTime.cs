using System;
using System.Collections.Generic;

namespace Utilme.SdpTransform
{
    public class RepeatTime
    {
        public TimeSpan RepeatInterval { get; set; }
        public TimeSpan ActiveDuration { get; set; }
        public IList<TimeSpan> OffsetsFromStartTime { get; set; }
    }
}
