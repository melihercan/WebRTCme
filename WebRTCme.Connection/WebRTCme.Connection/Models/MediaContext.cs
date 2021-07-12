using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public class MediaContext
    {
        public bool VideoMuted { get; init; }
        public bool AudioMuted { get; init; }
        public bool Speaking { get; init; }
    }
}
