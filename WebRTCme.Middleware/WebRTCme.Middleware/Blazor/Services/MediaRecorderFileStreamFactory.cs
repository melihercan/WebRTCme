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
    class MediaRecorderFileStreamFactory : IMediaRecorderFileStreamFactory
    {
        readonly IStreamSaver _streamSaver;
        readonly IJSRuntime _jsRuntime;

        public MediaRecorderFileStreamFactory(IStreamSaver streamSaver, IJSRuntime jsRuntime)
        {
            _streamSaver = streamSaver;
            _jsRuntime = jsRuntime;
        }

        public async Task<Stream> CreateStreamAsync(string fileName, MediaRecorderOptions options)
        {
            MediaRecorderFileStream mediaRecorderFileStream = new(fileName, options, _streamSaver);
            await mediaRecorderFileStream.Initialization;
            return mediaRecorderFileStream;
        }

        public async Task<BlobStream> CreateBlobStreamAsync(string fileName, MediaRecorderOptions options)
        {
            MediaRecorderBlobFileStream mediaRecorderBlobFileStream = new(fileName, options, _jsRuntime);
            await mediaRecorderBlobFileStream.Initialization;
            return mediaRecorderBlobFileStream;
        }

    }
}
