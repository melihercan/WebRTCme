using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware.Models
{
    internal class FileDto : BaseDto
    {
        public FileInfo FileInfo { get; init; }

        
        public byte[] Data { get; init; }
    }
}
