using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware.Services
{
    class MediaRecorderManager : IMediaRecorderManager
    {
        readonly IWebRtcMiddleware _webRtcMiddleware;
        readonly IMediaRecorderFileStreamFactory _mediaRecorderFileStreamFactory;
        readonly ILogger<MediaRecorderManager> _logger;
        readonly IJSRuntime _jsRuntime;

        List<MediaRecorderParameters> _mediaRecorderParametersList = new();

        public MediaRecorderManager(IWebRtcMiddleware webRtcMiddleware, 
            IMediaRecorderFileStreamFactory mediaRecorderFileStreamFactory,
            ILogger<MediaRecorderManager> logger, IJSRuntime jsRuntime = null)
        {
            _webRtcMiddleware = webRtcMiddleware;
            _mediaRecorderFileStreamFactory = mediaRecorderFileStreamFactory;
            _logger = logger;
            _jsRuntime = jsRuntime;
        }

        public async Task StartAsync(string fileName, int periodMs, IMediaStream mediaStream,
            MediaRecorderOptions mediaRecorderOptions)
        {
            if (GetParameters(fileName) is null)
                await StartOrStopRecordingAsync(new MediaRecorderParameters 
                { 
                    FileName = fileName,
                    PeriodMs = periodMs,
                    MediaStream = mediaStream,
                    MediaRecorderOptions = mediaRecorderOptions
                });
            else
                throw new Exception($"File {fileName} is already recording");
        }

        public async Task StopAsync(string fileName)
        {
            var mediaRecorderParameters = GetParameters(fileName);

            if (mediaRecorderParameters is not null)
                await StartOrStopRecordingAsync(mediaRecorderParameters, isStop: true);
            else
                throw new Exception($"File {fileName} is not recording");
        }

        public Task PauseAsync(string fileName)
        {
            var mediaRecorderParameters = GetParameters(fileName);

            if (mediaRecorderParameters is not null)
                mediaRecorderParameters.MediaRecorder.Pause();
            else
                throw new Exception($"File {fileName} is not recording");

            return Task.CompletedTask;
        }

        public Task ResumeAsync(string fileName)
        {
            var mediaRecorderParameters = GetParameters(fileName);

            if (mediaRecorderParameters is not null)
                mediaRecorderParameters.MediaRecorder.Resume();
            else
                throw new Exception($"File {fileName} is not recording");

            return Task.CompletedTask;
        }

        public async Task ResetAllAsync()
        {
            foreach (var mediaRecorderParameters in _mediaRecorderParametersList)
                await StopAsync(mediaRecorderParameters.FileName);
            _mediaRecorderParametersList.Clear();
        }

        MediaRecorderParameters GetParameters(string fileName) => _mediaRecorderParametersList
            .SingleOrDefault(mrp => mrp.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

        async Task StartOrStopRecordingAsync(MediaRecorderParameters mediaRecorderParameters, bool isStop = false)
        {
            if (isStop)
            {
                mediaRecorderParameters.MediaRecorder.OnStart -= MediaRecorder_OnStart;
                mediaRecorderParameters.MediaRecorder.OnStop -= MediaRecorder_OnStop;
                mediaRecorderParameters.MediaRecorder.OnPause -= MediaRecorder_OnPause;
                mediaRecorderParameters.MediaRecorder.OnResume -= MediaRecorder_OnResume;
                mediaRecorderParameters.MediaRecorder.OnDataAvailable -= MediaRecorder_OnDataAvailable;

                mediaRecorderParameters.BlobStream.Close();
                mediaRecorderParameters.MediaRecorder.Stop();
                mediaRecorderParameters.MediaRecorder.Dispose();

                _mediaRecorderParametersList.Remove(mediaRecorderParameters);
            }
            else
            {
                mediaRecorderParameters.BlobStream = await _mediaRecorderFileStreamFactory
                    .CreateBlobStreamAsync(mediaRecorderParameters.FileName, 
                        mediaRecorderParameters.MediaRecorderOptions);
                
                var window = _webRtcMiddleware.WebRtc.Window(_jsRuntime);

                mediaRecorderParameters.MediaRecorder = window.MediaRecorder(mediaRecorderParameters.MediaStream, 
                    mediaRecorderParameters.MediaRecorderOptions);
                mediaRecorderParameters.MediaRecorder.OnStart += MediaRecorder_OnStart;
                mediaRecorderParameters.MediaRecorder.OnStop += MediaRecorder_OnStop;
                mediaRecorderParameters.MediaRecorder.OnPause += MediaRecorder_OnPause;
                mediaRecorderParameters.MediaRecorder.OnResume += MediaRecorder_OnResume;
                mediaRecorderParameters.MediaRecorder.OnDataAvailable += MediaRecorder_OnDataAvailable;
                mediaRecorderParameters.MediaRecorder.Start(mediaRecorderParameters.PeriodMs);
                _mediaRecorderParametersList.Add(mediaRecorderParameters);
            }

            async void MediaRecorder_OnDataAvailable(object sender, IBlobEvent e)
            {
                var blob = e.Data;
                _logger.LogInformation($"---------------------------- {mediaRecorderParameters.FileName} RECORDER BLOB DATA: size:{blob.Size} type:{blob.Type }");

                await mediaRecorderParameters.BlobStream.WriteAsync(blob);
            }
            void MediaRecorder_OnStart(object sender, EventArgs e)
            {
                _logger.LogInformation($"---------------------------- {mediaRecorderParameters.FileName} RECORDER STARTED");
            }
            void MediaRecorder_OnStop(object sender, EventArgs e)
            {
                _logger.LogInformation($"---------------------------- {mediaRecorderParameters.FileName} RECORDER STOPPED");
            }
            void MediaRecorder_OnPause(object sender, EventArgs e)
            {
                _logger.LogInformation($"---------------------------- {mediaRecorderParameters.FileName} RECORDER PAUSED");
            }
            void MediaRecorder_OnResume(object sender, EventArgs e)
            {
                _logger.LogInformation($"---------------------------- {mediaRecorderParameters.FileName} RECORDER RESUMED");
            }
        }
    }
}
