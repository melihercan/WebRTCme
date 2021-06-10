using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices.JavaScript;

namespace WebRTCme.Bindings.Blazor.Api
{
    class MediaRecorder : IMediaRecorder
    {

        //HostObject
        public string MimeType => throw new NotImplementedException();

        public RecordingState State => throw new NotImplementedException();

        public IMediaStream Stream => throw new NotImplementedException();

        public bool IgnoreMutedMedia { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double VideoBitsPerSecond => throw new NotImplementedException();

        public double AudioBitsPerSecond => throw new NotImplementedException();

        public object NativeObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
