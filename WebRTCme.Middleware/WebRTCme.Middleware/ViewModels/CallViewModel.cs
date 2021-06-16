using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme.Middleware;

namespace WebRTCme.Middleware
{
    public class CallViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // A reference is required here. otherwise binding does not work.
        public ObservableCollection<MediaParameters> MediaParametersList { get; set; }

        private readonly IMediaStreamService _mediaStreamService;
        private readonly IWebRtcConnection _webRtcConnection;
        private readonly ISignallingServerService _signallingServerService;
        private readonly IMediaManagerService _mediaManagerService;
        private readonly IVideoRecorderFileStreamFactory _videoRecorderFileStreamFactory;
        private readonly INavigationService _navigationService;
        private readonly IRunOnUiThreadService _runOnUiThreadService;
        private readonly ILogger<CallViewModel> _logger;
        private readonly IJSRuntime _jsRuntime;

        private IDisposable _connectionDisposer;
        private Action _reRender;
        private string _userName;
        private IMediaStream _cameraStream;
        private IMediaStream _displayStream;
        private ConnectionParameters _connectionParameters;

        private IMediaRecorder _mediaRecorder;

        Stream _videoRecorderFileStream;


        public CallViewModel(IMediaStreamService mediaStreamService, IWebRtcConnection webRtcConnection,
            ISignallingServerService signallingServerService, IMediaManagerService mediaManagerService,
            IVideoRecorderFileStreamFactory videoRecorderFileStreamFactory, INavigationService navigationService,  
            IRunOnUiThreadService runOnUiThreadService, ILogger<CallViewModel> logger, IJSRuntime jsRuntime = null)
        {
            _mediaStreamService = mediaStreamService;
            _webRtcConnection = webRtcConnection;
            _signallingServerService = signallingServerService;
            _mediaManagerService = mediaManagerService;
            _videoRecorderFileStreamFactory = videoRecorderFileStreamFactory;
            _navigationService = navigationService;
            _runOnUiThreadService = runOnUiThreadService;
            _logger = logger;
            _jsRuntime = jsRuntime;
            MediaParametersList = mediaManagerService.MediaParametersList;
        }

        public async Task OnPageAppearingAsync(ConnectionParameters connectionParameters, Action reRender = null)
        {
            _connectionParameters = connectionParameters;
            _reRender = reRender;
            _userName = connectionParameters.UserName;
            _cameraStream = await _mediaStreamService.GetCameraMediaStreamAsync();
            _displayStream = await _mediaStreamService.GetDisplayMediaStreamAync();
            _mediaManagerService.Add(new MediaParameters
            {
                Label = connectionParameters.UserName,
                Stream = _cameraStream,
                VideoMuted = false,
                AudioMuted = false,
                ShowControls = false
            });

            reRender?.Invoke();

            var connectionRequestParameters = new ConnectionRequestParameters
            {
                ConnectionParameters = connectionParameters,
                LocalStream = _cameraStream,
            };
            Connect(connectionRequestParameters);
        }

        public Task OnPageDisappearingAsync()
        {
            Disconnect();
            return Task.CompletedTask;
        }

        private void Connect(ConnectionRequestParameters connectionRequestParameters)
        {
            _connectionDisposer = _webRtcConnection.ConnectionRequest(connectionRequestParameters).Subscribe(
                onNext: (peerResponseParameters) =>
                {
                    switch (peerResponseParameters.Code)
                    {
                        case PeerResponseCode.PeerJoined:
                            if (peerResponseParameters.MediaStream != null)
                            {
                                _runOnUiThreadService.Invoke(() =>
                                {
                                    _mediaManagerService.Add(new MediaParameters
                                    {
                                        Label = peerResponseParameters.PeerUserName,
                                        Stream = peerResponseParameters.MediaStream,
                                        VideoMuted = false,
                                        AudioMuted = false,
                                        ShowControls = false
                                    });
                                });

                                _reRender?.Invoke();
                            }
                            break;

                        case PeerResponseCode.PeerLeft:
                            _runOnUiThreadService.Invoke(() =>
                            {
                                _mediaManagerService.Remove(peerResponseParameters.PeerUserName);
                            });
                            _reRender?.Invoke();
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerLeft");
                            break;

                        case PeerResponseCode.PeerError:
                            _runOnUiThreadService.Invoke(() =>
                            {
                                _mediaManagerService.Remove(peerResponseParameters.PeerUserName);
                            });
                            _reRender?.Invoke();
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerError");
                            break;
                    }
                },
                onError: (exception) =>
                {
                    System.Diagnostics.Debug.WriteLine($"************* APP OnError:{exception.Message}");
                },
                onCompleted: () =>
                {
                    System.Diagnostics.Debug.WriteLine($"************* APP OnCompleted");
                });
        }

        private void Disconnect()
        {
            _mediaManagerService.Clear();
            _connectionDisposer.Dispose();
        }

        private bool _isSharingScreen;
        private string _shareScreenButtonText = "Start sharing screen";
        public string ShareScreenButtonText
        {
            get => _shareScreenButtonText;
            set
            {
                _shareScreenButtonText = value;
                OnPropertyChanged();
            }
        }

        public async Task OnShareScreenAsync()
        {
            if (_isSharingScreen)
            {
                // Stop sharing.
                ShareScreenButtonText = "Start sharing screen";
                await _webRtcConnection.ReplaceOutgoingVideoTracksAsync(_connectionParameters.TurnServerName,
                    _connectionParameters.RoomName, _cameraStream.GetVideoTracks()[0]);

            }
            else
            {
                // Start sharing.
                ShareScreenButtonText = "Stop sharing screen";
                await _webRtcConnection.ReplaceOutgoingVideoTracksAsync(_connectionParameters.TurnServerName,
                    _connectionParameters.RoomName, _displayStream.GetVideoTracks()[0]);
            }
            _isSharingScreen = !_isSharingScreen;

        }

        public ICommand ShareScreenCommand => new AsyncCommand(async () =>
        {
            await OnShareScreenAsync();
        });

        private bool _isRecording;
        private string _recordButtonText = "Start recording";
        public string RecordButtonText
        {
            get => _recordButtonText;
            set
            {
                _recordButtonText = value;
                OnPropertyChanged();
            }
        }

        public async Task OnRecordAsync()
        {
            if (_isRecording)
            {
                // Stop recording.
                RecordButtonText = "Start recording";


////                _mediaRecorder.Pause();
////                _mediaRecorder.RequestData();
////                await Task.Delay(1000);

                _videoRecorderFileStream.Close();
                _mediaRecorder.Stop();
                _mediaRecorder.Dispose();
            }
            else
            {
                // Start recording.
                RecordButtonText = "Stop recording";

                var mediaRecorderOptions = new MediaRecorderOptions
                {
                    MimeType = "video/webm",
                    //MimeType = "video/webm;codecs=vp9",
                    //MimeType = "video/webm;codecs=h264,opus",
                    //AudioBitsPerSecond = 48000,
                    //VideoBitsPerSecond = 2500000,
                };

                _videoRecorderFileStream = await _videoRecorderFileStreamFactory
                    .CreateStreamAsync("MyFirst.webm", mediaRecorderOptions);

                var window = WebRtcMiddleware.WebRtc.Window(_jsRuntime);

                _mediaRecorder = window.MediaRecorder(/****_displayStream****/ _cameraStream, mediaRecorderOptions); 
                _mediaRecorder.OnDataAvailable += async (s, e) => 
                {
                    var blob = e.Data;
                    _logger.LogInformation($"---------------------------- RECORDER BLOB DATA: size:{blob.Size} type:{blob.Type }");

                    ////var newBlob = blob.Slice(0, e.Data.Size, "video/webm");
                    ////_logger.LogInformation($"---------------------------- RECORDER NEW BLOB DATA: size:{newBlob.Size} type:{newBlob.Type }");

                    //_videoRecorderFileStream.Close();
                    //_mediaRecorder.Stop();
                    // _mediaRecorder.Dispose();


                    //var dataBin = await blob.ArrayBuffer();

                    _logger.LogInformation($"---------------------------- NOW: {DateTime.Now}");
                    var data = await blob.Text();
                    //var data = await newBlob.Text();
                    //_logger.LogInformation($"---------------------------- RECORDER UTF-8 STRING : size:{data.Length}");


                    _logger.LogInformation($"---------------------------- THEN: {DateTime.Now}");
                    var dataBin = Encoding.UTF8.GetBytes(data);
                    ////                    _logger.LogInformation($"DATA:\n {data}");
                    //_mediaRecorder.Stop();
                    //var b64 = Convert.ToBase64String(dataBin);
                    //_logger.LogInformation($"---------------------------- RECORDER Base64 STRING : {b64}");


                    _logger.LogInformation($"---------------------------- RECORDER BYTES : size:{dataBin.Length}");
                    await _videoRecorderFileStream.WriteAsync(dataBin, 0, dataBin.Length);




                };
                _mediaRecorder.OnStart += (s, e) => 
                {
                    _logger.LogInformation("---------------------------- RECORDER STARTED");
                };
                _mediaRecorder.Start(3000);





                //await Task.Delay(1000);
                //_mediaRecorder.RequestData();
    ///_mediaRecorder.Stop();
            }
            _isRecording = !_isRecording;

            //return Task.CompletedTask;
        }
    }
}
