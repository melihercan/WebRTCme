using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware.Models
{
    internal class FileDto : BaseDto
    {
        public string Name { get; init; }
        public ulong Size { get; init; }
        public string ContentType { get; init; }
        public ulong Offset { get; init; }
        public byte[] Data { get; init; }
    }
}
