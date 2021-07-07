using System;
using System.Collections.Generic;

namespace SDPLib
{
    public class RepeatTime
    {
        public TimeSpan RepeatInterval { get; set; }
        public TimeSpan ActiveDuration { get; set; }
        public IList<TimeSpan> OffsetsFromStartTime { get; set; }
    }
}
