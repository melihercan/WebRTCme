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
        public const string Name = "candidate:";

        /// <summary>
        /// It is <ice-char>
        /// ice-char: ALPHA / DIGIT / "+" / "/"
        /// </summary>
        public string Foundation { get; init; }

        public int ComponentId { get; init; }

        public Transport Transport { get; init; }

        public int Priority { get; init; }

        public string ConnectionAddress { get; init; }

        public int Port { get; init; }

        public const string Typ = "typ";

        public CandidateType Type { get; init; }

        public const string Raddr = "raddr";
        public string RelAddr { get; init; }

        public const string Rport = "rport";
        public int RelPort { get; init; }



    }
}
