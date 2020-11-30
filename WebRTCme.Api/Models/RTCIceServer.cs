using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class RTCIceServer
    {
        public string Credential { get; set; }

        public RTCIceCredentialType? CredentialType { get; set; }
        
        public string[] Urls { get; set; }
        
        public string Username { get; set; }

    }
}
