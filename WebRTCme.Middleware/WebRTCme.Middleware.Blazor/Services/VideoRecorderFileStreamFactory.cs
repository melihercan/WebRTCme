using Blazorme;
using Microsoft.JSInterop;
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
        readonly IStreamSaver _streamSaver;
        readonly IJSRuntime _jsRuntime;

        public VideoRecorderFileStreamFactory(IStreamSaver streamSaver, IJSRuntime jsRuntime)
        {
            _streamSaver = streamSaver;
            _jsRuntime = jsRuntime;
        }

        public async Task<Stream> CreateStreamAsync(string fileName, MediaRecorderOptions options)
        {
            VideoRecorderFileStream videoRecorderFileStream = new(fileName, options, _streamSaver);
            await videoRecorderFileStream.Initialization;
            return videoRecorderFileStream;
        }

        public async Task<BlobStream> CreateBlobStreamAsync(string fileName, MediaRecorderOptions options)
        {
            VideoRecorderBlobFileStream videoRecorderBlobFileStream = new(fileName, options, _jsRuntime);
            await videoRecorderBlobFileStream.Initialization;
            return videoRecorderBlobFileStream;
        }

    }
}
