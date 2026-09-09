using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IMediaRecorderManager
    {
        Task StartAsync(string fileName, int periodMs, IMediaStream mediaStream,
            MediaRecorderOptions mediaRecorderOptions);
        Task StopAsync(string fileName);
        Task PauseAsync(string fileName);
        Task ResumeAsync(string fileName);

        Task ResetAllAsync();
    }
}
