using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IMediaRecorder : IDisposable // INativeObject
    {
        string MimeType { get; } 
        
        RecordingState State { get; }

        IMediaStream Stream { get; }

        bool IgnoreMutedMedia { get; set; }

        double VideoBitsPerSecond { get; }

        double AudioBitsPerSecond { get; }


        event EventHandler<IBlobEvent> OnDataAvailable;
        event EventHandler<IDOMException> OnError;
        event EventHandler OnPause;
        event EventHandler OnResume;
        event EventHandler OnStart;
        event EventHandler OnStop;


        void Pause();

        void RequestData();

        void Resume();

        void Start(int? timeslice = null);

        void Stop();

        /*static*/
        bool IsTypeSupported(string mimeType);


    }
}
