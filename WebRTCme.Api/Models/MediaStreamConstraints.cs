using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class MediaStreamConstraints
    {
        public bool Audio { get; set; }
        //// TODO: or IMediaTrackConstraints

        public bool Video { get; set; }
        //// TODO: or IMediaTrackConstraints

        //// TODO: public string PeerIdentity { get; set; } 
    }
}
