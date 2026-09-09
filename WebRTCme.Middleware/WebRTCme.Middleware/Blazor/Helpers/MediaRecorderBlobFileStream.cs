using Blazorme;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;

namespace WebRTCme.Middleware.Blazor.Helpers
{
    class MediaRecorderBlobFileStream : BlobStream, IAsyncInit
    {
        readonly string _fileName;
        readonly MediaRecorderOptions _mediaRecorderOptions;
        readonly IJSRuntime _jsRuntime;
        readonly JsObjectRef _streamSaverJsObjectRef;
        readonly JsObjectRef _writeableStreamJsObjectRef;
        readonly JsObjectRef _writerJsObjectRef;

        Stream _writableFileStream;

        public MediaRecorderBlobFileStream(string fileName, MediaRecorderOptions mediaRecorderOptions, 
            IJSRuntime jsRuntime)
        {
            _fileName = fileName;
            _mediaRecorderOptions = mediaRecorderOptions;
            _jsRuntime = jsRuntime;

            _streamSaverJsObjectRef = jsRuntime.GetJsPropertyObjectRef("window", "streamSaver");
            _writeableStreamJsObjectRef = jsRuntime.CallJsMethod<JsObjectRef>(
                _streamSaverJsObjectRef, "createWriteStream", fileName);
            _writerJsObjectRef = jsRuntime.CallJsMethod<JsObjectRef>(_writeableStreamJsObjectRef, "getWriter");

            Initialization = InitAsync();
        }

        public Task Initialization { get; private set; }

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position 
        { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }

        Task InitAsync()
        {
            return Task.CompletedTask;
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

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteAsync(IBlob blob, CancellationToken cancellationToken)
        {
            var arrayBufferJsObject = await _jsRuntime.CallJsMethodAsync<JsObjectRef>(blob.GetNativeObject(), 
                "arrayBuffer");
            var uint8JsObject = _jsRuntime.CreateJsObject("window", "Uint8Array", arrayBufferJsObject);
            await _jsRuntime.CallJsMethodVoidAsync(_writerJsObjectRef, "write", uint8JsObject);
        }

        public override void Close()
        {
            Task.Run(async () => await _jsRuntime.CallJsMethodVoidAsync(_writerJsObjectRef, "close"));
            base.Close();
        }
    }
}
