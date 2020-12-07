using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public class RTCIceParameters
    {
        public string UsernameFragment { get; set; }

        public string Password { get; set; }
    }
}
