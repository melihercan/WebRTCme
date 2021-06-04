using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware.Models
{
    [Serializable]
    internal class FileDto : BaseDto
    {
        public FileInfo FileInfo { get; init; }
        
        // Zero lenght indicates end of file transfer.
        public byte[] Data { get; init; }
    }
}
