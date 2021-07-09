using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class IceParameters
    {
        public string UsernameFragment { get; init; }
        public string Password { get; init; }
        public bool? IceLite { get; init; }
    }
}
