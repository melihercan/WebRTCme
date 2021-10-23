using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    // Example:
    //  a=candidate:2 1 UDP 1694498815 192.0.2.3 45664 typ srflx raddr 10.0.1.1 rport 8998
    public class Candidate
    {
        public const string Label = "candidate:";

        public string Foundation { get; set; }

        public int ComponentId { get; set; }

        public CandidateTransport Transport { get; set; }

        public int Priority { get; set; }

        public string ConnectionAddress { get; set; }

        public int Port { get; set; }

        public const string Typ = "typ";
        public CandidateType Type { get; set; }


        // Both are optional.
        public const string Raddr = "raddr";
        public string RelAddr { get; set; }

        public const string Rport = "rport";
        public int? RelPort { get; set; }

        // Optional extensions, name value pairs separated with space.
        public (string, string)[] Extensions { get; set; }


    }
}
