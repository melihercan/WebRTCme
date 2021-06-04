using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware
{
    public class File
    {
        public string Name { get; init; }
        public ulong Size { get; init; }
        public string ContentType { get; init; }
        public Stream Stream { get; init; }
    }
}
