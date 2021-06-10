using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.JSInterop;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class MediaRecorder : ApiBase, IMediaRecorder
    {
        //// TODO: REFACTOR WHOLE BLAZOR API BY USING System.Private.Runtime.InteropServices.JavaScript and use HostObject.
        //private readonly JSObject _jsObjectMediaRecorder;
        ////private readonly IMediaStream _stream;
        ////private readonly MediaRecorderOptions _options;

        //public MediaRecorder(IMediaStream stream, MediaRecorderOptions options)
        //{
            //_stream = stream;
            //_options = options;
            //_jsObjectMediaRecorder = new HostObject("MediaRecorder");
        //}

        public static IMediaRecorder Create(IJSRuntime jsRuntime, IMediaStream stream, MediaRecorderOptions options)
        {
            var jsObjectRef = jsRuntime.CreateJsObject("window", "MediaRecorder", stream.NativeObject, options);
            return new MediaRecorder(jsRuntime, jsObjectRef, stream, options);
        }

        private MediaRecorder(IJSRuntime jsRuntime, JsObjectRef jsObjectRef, IMediaStream stream, 
            MediaRecorderOptions options) : base(jsRuntime, jsObjectRef)
        {
            ////AddNativeEventListenerForObjectRef("dataavailable", (s, e) => OnDataAvailable?.Invoke(s, e),
                //// TCDataChannelEvent.Create);

        }

        public string MimeType => throw new NotImplementedException();

        public RecordingState State => throw new NotImplementedException();

        public IMediaStream Stream => throw new NotImplementedException();

        public bool IgnoreMutedMedia { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double VideoBitsPerSecond => throw new NotImplementedException();

        public double AudioBitsPerSecond => throw new NotImplementedException();

        ////public object NativeObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<IBlobEvent> OnDataAvailable;
        public event EventHandler<IDOMException> OnError;
        public event EventHandler OnPause;
        public event EventHandler OnResume;
        public event EventHandler OnStart;
        public event EventHandler OnStop;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTypeSupported(string mimeType)
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void RequestData()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Start(int? timeslice = null)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
