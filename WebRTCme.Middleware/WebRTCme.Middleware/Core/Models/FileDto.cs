using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware.Models
{
    internal class FileDto : BaseDto
    {
        public Guid Guid { get; init; }
        public string Name { get; init; }
        public ulong Size { get; init; }
        public ulong Offset { get; init; }
        public string ContentType { get; init; }
        public byte[] Data { get; init; }
    }
}
