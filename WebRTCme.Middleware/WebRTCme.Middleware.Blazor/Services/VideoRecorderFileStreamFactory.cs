using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware.Blazor.Services
{
    class VideoRecorderFileStreamFactory : IVideoRecorderFileStreamFactory
    {
        public Task<Stream> CreateAsync(string fileName, MediaRecorderOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
