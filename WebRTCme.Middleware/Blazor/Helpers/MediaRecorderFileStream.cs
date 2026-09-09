using Blazorme;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebRTCme.Middleware.Blazor.Helpers
{
    class MediaRecorderFileStream : Stream, IAsyncInit
    {
        readonly string _fileName;
        readonly MediaRecorderOptions _mediaRecorderOptions;
        readonly IStreamSaver _streamSaver;

        Stream _writableFileStream;

        public MediaRecorderFileStream(string fileName, MediaRecorderOptions mediaRecorderOptions, IStreamSaver streamSaver)
        {
            _fileName = fileName;
            _mediaRecorderOptions = mediaRecorderOptions;
            _streamSaver = streamSaver;

            Initialization = InitAsync();
        }

        public Task Initialization { get; private set; }

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        async Task InitAsync()
        {
            _writableFileStream = await _streamSaver.CreateWritableFileStreamAsync(_fileName);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _writableFileStream.WriteAsync(buffer, 0, count, cancellationToken);
        }

        public override void Close()
        {
            Task.Run(async () => await _writableFileStream.DisposeAsync());
            base.Close();
        }
    }
}
