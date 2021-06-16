using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using WebRTCme.Middleware.Models;
using WebRTCme.Middleware.Services;

namespace WebRTCme.Middleware.Helpers
{
    internal class WebRtcOutgoingFileStream : Stream
    {
        private readonly File _file;
        private readonly Action<object> _sendObject;
        private readonly ILogger<DataManager> _logger;
        private Guid _guid;

        public WebRtcOutgoingFileStream(File file, Action<object> sendObject, ILogger<DataManager> logger)
        {
            _file = file;
            _sendObject = sendObject;
            _logger = logger;
            _guid = Guid.NewGuid();

            Length = (long)file.Size;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length { get; }

        public override long Position { get; set; }

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
            _logger.LogInformation($"============== WRITING {_file.Name} offset:{Position} count:{count}");

            var data = buffer.Skip(offset).Take(count).ToArray();

            FileDto fileDto = new()
            {
                Cookie = DataManager.Cookie,
                DtoObjectType = Enums.DataObjectType.File,
                Guid = _guid,
                Name = _file.Name,
                Size = _file.Size,
                Offset = (ulong)Position,
                ContentType = _file.ContentType,
                Data = data
            };
            var json = JsonSerializer.Serialize(fileDto);
            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);
            _sendObject(base64);

            Position += count;
        }

    }
}
