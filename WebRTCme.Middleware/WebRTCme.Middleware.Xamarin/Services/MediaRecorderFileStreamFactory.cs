using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware.Xamarin.Services
{
    class MediaRecorderFileStreamFactory : IMediaRecorderFileStreamFactory
    {

        public Task<Stream> CreateStreamAsync(string fileName, MediaRecorderOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<BlobStream> CreateBlobStreamAsync(string fileName, MediaRecorderOptions options)
        {
            throw new NotImplementedException();
        }

    }
}
