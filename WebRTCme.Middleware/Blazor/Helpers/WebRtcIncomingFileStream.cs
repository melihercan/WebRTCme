using Blazorme;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebRTCme.Middleware.Blazor.Helpers
{
    class WebRtcIncomingFileStream : Stream
    {
        private readonly IStreamSaver _streamSaver;
        private readonly string _peerUserName;
        private readonly File _file;
        private readonly DataParameters _dataParameters;
        private readonly Action<string/*peerUserName*/, Guid/*fileGuid*/> _onCompleted;
        private Stream _writableFileStream;

        public WebRtcIncomingFileStream(IStreamSaver streamSaver, string peerUserName, 
            DataParameters dataParameters, Action<string, Guid> onCompleted)
        {
            _streamSaver = streamSaver;
            _peerUserName = peerUserName;
            _file = dataParameters.Object as File;
            _dataParameters = dataParameters;
            _onCompleted = onCompleted;

            Length = (long)_file.Size;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length { get; }

        public override long Position { get; set; }

        public async Task CreateAsync()
        {
            _writableFileStream = await _streamSaver.CreateWritableFileStreamAsync(_file.Name);
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
            Position += count;
            //// TODO: GUI PROGRESS BAR PROCESS BY USING _dataParameters
            if (Position >= Length)
            {
                // File download completed, we are done.
                await _writableFileStream.DisposeAsync();
                _onCompleted(_peerUserName, _file.Guid);
                Dispose();
            }
        }
    }
}
