using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class MediaStreamConstraints
    {
        public bool Audio { get; set; }
        public MediaTrackConstraints AudioConstraints { get; set; }

        public bool Video { get; set; }
        public MediaTrackConstraints VideoConstraints { get; set; }

        public string PeerIdentity { get; set; } 
    }
}
