using Blazorme;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware.Blazor.Helpers;

namespace WebRTCme.Middleware.Blazor.Services
{
    class VideoRecorderFileStreamFactory : IVideoRecorderFileStreamFactory
    {
//// TEMPORARY FOR TESTING
readonly IStreamSaver _streamSaver;
   public VideoRecorderFileStreamFactory(IStreamSaver streamSaver)
   {
        _streamSaver = streamSaver;
   }


        public async Task<Stream> CreateStreamAsync(string fileName, MediaRecorderOptions options)
        {
            VideoRecorderFileStream videoRecorderFileStream = new(fileName, options);
            await videoRecorderFileStream.CreateAsync(_streamSaver);
            return videoRecorderFileStream;
        }

        public async Task<BlobStream> CreateBlobStreamAsync(string fileName, MediaRecorderOptions options)
        {
            VideoRecorderBlobFileStream videoRecorderBlobFileStream = new(fileName, options);
            await videoRecorderBlobFileStream.CreateAsync(_streamSaver);
            return videoRecorderBlobFileStream;
        }

    }
}
