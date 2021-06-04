using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware.Models
{
    [Serializable]
    internal class FileDto : BaseDto
    {
        public string Name { get; init; }
        public ulong Size { get; init; }
        public string ContentType { get; init; }

        // Zero lenght indicates end of file transfer.
        public ulong Offset { get; init; }
        public byte[] Data { get; init; }
    }
}
