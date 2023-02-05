using Microsoft.JSInterop;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class MediaRecorder : NativeBase, IMediaRecorder
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

        public MediaRecorder(IJSRuntime jsRuntime, IMediaStream stream, MediaRecorderOptions options) : 
            this(jsRuntime,
                jsRuntime.CreateJsObject("window", "MediaRecorder", ((MediaStream)stream).NativeObject, options), 
                stream, 
                options)
        {

        }

        public MediaRecorder(IJSRuntime jsRuntime, JsObjectRef jsObjectRef, IMediaStream stream, 
            MediaRecorderOptions options) : base(jsRuntime, jsObjectRef)
        {
            AddNativeEventListenerForObjectRef("dataavailable", (s, e) => OnDataAvailable?.Invoke(s, e),
                BlobEvent.Create);
            AddNativeEventListenerForObjectRef("error", (s, e) => OnError?.Invoke(s, e),
                DOMException.Create);
            AddNativeEventListener("pause", (s, e) => OnPause?.Invoke(s, e));
            AddNativeEventListener("resume", (s, e) => OnResume?.Invoke(s, e));
            AddNativeEventListener("start", (s, e) => OnStart?.Invoke(s, e));
            AddNativeEventListener("stop", (s, e) => OnStop?.Invoke(s, e));
        }

        public string MimeType => GetNativeProperty<string>("mimeType");

        public RecordingState State => GetNativeProperty<RecordingState>("state");

        public IMediaStream Stream => 
            new MediaStream(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "stream"));

        public bool IgnoreMutedMedia 
        { 
            get => GetNativeProperty<bool>("ignoreMutedMedia"); 
            set => SetNativeProperty("ignoreMutedMedia", value);
        }

        public double VideoBitsPerSecond => GetNativeProperty<double>("videoBitsPerSecond");

        public double AudioBitsPerSecond => GetNativeProperty<double>("AudioBitsPerSecond");


        public event EventHandler<IBlobEvent> OnDataAvailable;
        public event EventHandler<IDOMException> OnError;
        public event EventHandler OnPause;
        public event EventHandler OnResume;
        public event EventHandler OnStart;
        public event EventHandler OnStop;

        public bool IsTypeSupported(string mimeType)
        {
            return JsRuntime.CallJsMethod<bool>(NativeObject, mimeType);
        }

        public void Pause()
        {
            JsRuntime.CallJsMethodVoid(NativeObject, "pause");
        }

        public void RequestData()
        {
            JsRuntime.CallJsMethodVoid(NativeObject, "requestData");
        }

        public void Resume()
        {
            JsRuntime.CallJsMethodVoid(NativeObject, "resume");
        }

        public void Start(int? timeslice = null)
        {
            JsRuntime.CallJsMethodVoid(NativeObject, "start", timeslice);
        }

        public void Stop()
        {
            JsRuntime.CallJsMethodVoid(NativeObject, "stop");
        }
    }
}
