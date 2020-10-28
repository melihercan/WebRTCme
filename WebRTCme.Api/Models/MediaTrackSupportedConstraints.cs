using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class MediaTrackSupportedConstraints
    {
        public bool AutoGainControl { get; set; }
        public bool Width { get; set; }
        public bool Height { get; set; }
        public bool AspectRatio { get; set; }
        public bool FrameRate { get; set; }
        public bool FacingMode { get; set; }
        public bool ResizeMode { get; set; }
        public bool Volume { get; set; }
        public bool SampleRate { get; set; }
        public bool SampleSize { get; set; }
        public bool EchoCancellation { get; set; }
        public bool Latency { get; set; }
        public bool NoiseSuppression { get; set; }
        public bool ChannelCount { get; set; }
        public bool DeviceId { get; set; }
        public bool GroupId { get; set; }
        public bool Cursor { get; set; }
        public bool DisplaySurface { get; set; }
        public bool LogicalSurface { get; set; }
    }
}
