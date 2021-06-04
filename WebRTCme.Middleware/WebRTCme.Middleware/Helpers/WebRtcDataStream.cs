using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware.Helpers
{
    internal class WebRtcDataStream : Stream
    {
        private readonly DataParameters _dataParameters;
        private readonly IRTCDataChannel _dataChannel;
        public WebRtcDataStream(DataParameters dataParameters, IRTCDataChannel dataChannel)
        {
            _dataParameters = dataParameters;
            _dataChannel = dataChannel;
        }

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
    }
}
