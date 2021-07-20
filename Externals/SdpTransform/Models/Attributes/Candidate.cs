using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    public class Candidate
    {
        public const string Name = "candidate";

        /// <summary>
        /// It is <ice-char>
        /// ice-char: ALPHA / DIGIT / "+" / "/"
        /// </summary>
        public string Foundation { get; init; }

        [JsonPropertyName("component-id")]
        public int ComponentId { get; init; }

        public Transport Transport { get; init; }

        public int Priority { get; init; }

        public string ConnectionAddress { get; init; }

        public int Port { get; init; }

        public string Typ = "typ";

        public CandidateType Type { get; init; }

        public string Raddr = "raddr";
        public string RelAddr { get; init; }

        public string Rport = "rport";
        public int RelPort { get; init; }



    }
}
