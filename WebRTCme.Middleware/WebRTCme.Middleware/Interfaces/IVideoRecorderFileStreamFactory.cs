using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IVideoRecorderFileStreamFactory
    {
        Task<Stream> CreateStreamAsync(string fileName, MediaRecorderOptions options);

        Task<BlobStream> CreateBlobStreamAsync(string fileName, MediaRecorderOptions options);
    }
}
