using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IVideoRecorderFileStreamFactory
    {
        Task<Stream> CreateAsync(string fileName, MediaRecorderOptions options);
    }
}
