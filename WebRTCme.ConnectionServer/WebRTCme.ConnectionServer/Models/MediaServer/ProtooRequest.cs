using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.ConnectionServer
{
    public class ProtooRequest
    {
        public bool Request { get; init; } = true;
        public uint Id { get; init; }
        public string Method { get; init; }
        public object Data { get; init; }
    }
}
