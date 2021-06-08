using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware
{
    public class File
    {
        public Guid Guid { get; init; }
        public string Name { get; init; }
        public string ContentType { get; init; }
        public ulong Size { get; init; }

        //public ulong Offset { get; init; }

        //public Stream Stream { get; init; }
    }
}
