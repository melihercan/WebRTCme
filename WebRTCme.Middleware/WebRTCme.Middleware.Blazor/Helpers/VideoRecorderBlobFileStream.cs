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
    class VideoRecorderBlobFileStream : BlobStream
    {
        readonly string _fileName;
        readonly MediaRecorderOptions _mediaRecorderOptions;

IStreamSaver _streamSaver;
Stream _writableFileStream;

        public VideoRecorderBlobFileStream(string fileName, MediaRecorderOptions mediaRecorderOptions)
        {
            _fileName = fileName;
            _mediaRecorderOptions = mediaRecorderOptions;
        }


        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public async Task CreateAsync(/*** TESTING*/IStreamSaver streamSaver)
        {
            //// TODO: CREATE HERE FFmpeg access
            ///


            /////////////// TEMPORARY SAVE EACH BLOB TO A FILE
    _streamSaver = streamSaver;
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
