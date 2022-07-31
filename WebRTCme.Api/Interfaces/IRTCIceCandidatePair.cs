using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCIceCandidatePair : IDisposable // INativeObject
    {
        public IRTCIceCandidate Local { get; set; }

        public IRTCIceCandidate Remote { get; set; }
    }
}
