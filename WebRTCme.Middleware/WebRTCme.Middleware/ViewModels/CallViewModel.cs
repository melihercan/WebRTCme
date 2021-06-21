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
        void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // A reference is required here. otherwise binding does not work.
        public ObservableCollection<MediaStreamParameters> MediaStreamParametersList { get; set; }

        readonly INavigation _navigation;
        readonly ILocalMediaStream _localMediaStream;
        readonly IWebRtcConnection _webRtcConnection;
        readonly IMediaStreamManager _mediaStreamManager;
        readonly IMediaRecorderManager _mediaRecorderManager;
//        readonly IMediaRecorderFileStreamFactory _mediaRecorderFileStreamFactory;
        readonly IModalPopup _modalPopup;
        readonly IRunOnUiThread _runOnUiThread;
        readonly ILogger<CallViewModel> _logger;
        readonly IJSRuntime _jsRuntime;

        IDisposable _connectionDisposer;
        Action _reRender;
        //string _userName;
        IMediaStream _cameraStream;
        IMediaStream _displayStream;
        ConnectionParameters _connectionParameters;

        //IMediaRecorder _mediaRecorder;

        //BlobStream _mediaRecorderBlobFileStream;

        string _recordingFileName = "WebRTCme.webm";

        public CallViewModel(INavigation navigation, ILocalMediaStream localMediaStream, 
            IWebRtcConnection webRtcConnection, IMediaStreamManager mediaStreamManager,
            IMediaRecorderManager mediaRecorderManager,
            //IMediaRecorderFileStreamFactory mediaRecorderFileStreamFactory, 
            IModalPopup modalPopup, 
            IRunOnUiThread runOnUiThreadService, ILogger<CallViewModel> logger, 
            IJSRuntime jsRuntime = null)
        {
            _navigation = navigation;
            _localMediaStream = localMediaStream;
            _webRtcConnection = webRtcConnection;
            _mediaStreamManager = mediaStreamManager;
            _mediaRecorderManager = mediaRecorderManager;
            //_mediaRecorderFileStreamFactory = mediaRecorderFileStreamFactory;
            _modalPopup = modalPopup;
            _runOnUiThread = runOnUiThreadService;
            _logger = logger;
            _jsRuntime = jsRuntime;
            MediaStreamParametersList = mediaStreamManager.MediaStreamParametersList;
        }

        public async Task OnPageAppearingAsync(ConnectionParameters connectionParameters, Action reRender = null)
        {
            _connectionParameters = connectionParameters;
            _reRender = reRender;
            //_userName = connectionParameters.UserName;
            _cameraStream = await _localMediaStream.GetCameraMediaStreamAsync();
            _mediaStreamManager.Add(new MediaStreamParameters
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
                // 'async' here is fire-and-forget!!! It is OK for exceptions and error messages only.
                onNext: (Action<PeerResponseParameters>)(async peerResponseParameters =>
                {
                    switch (peerResponseParameters.Code)
                    {
                        case PeerResponseCode.PeerJoined:
                            if (peerResponseParameters.MediaStream != null)
                            {
                                _runOnUiThread.Invoke((Action)(() =>
                                {
                                    _mediaStreamManager.Add((MediaStreamParameters)new MediaStreamParameters
                                    {
                                        Label = peerResponseParameters.PeerUserName,
                                        Stream = peerResponseParameters.MediaStream,
                                        VideoMuted = false,
                                        AudioMuted = false,
                                        ShowControls = false
                                    });
                                }));

                                _reRender?.Invoke();
                            }
                            break;

                        case PeerResponseCode.PeerLeft:
                            _runOnUiThread.Invoke(() =>
                            {
                                _mediaStreamManager.Remove(peerResponseParameters.PeerUserName);
                            });
                            _reRender?.Invoke();
                            _logger.LogInformation($"************* APP PeerLeft");
                            break;

                        case PeerResponseCode.PeerError:
                            _runOnUiThread.Invoke(() =>
                            {
                                _mediaStreamManager.Remove(peerResponseParameters.PeerUserName);
                            });
                            _reRender?.Invoke();

                            _logger.LogInformation($"************* APP PeerError");
                            _ = await _modalPopup.GenericPopupAsync(new GenericPopupIn
                            {
                                Title = "Error",
                                Text = $"Peer {peerResponseParameters.PeerUserName} indicated an error:" +
                                       Environment.NewLine +
                                       peerResponseParameters.ErrorMessage,
                                Ok = "Ok",
                            });
                            break;
                    }
                }),
                onError: async exception =>
                {
                    _logger.LogInformation($"************* APP OnError:{exception.Message}");
                    if (exception.Message.Equals($"{SignallingServerProxy.SignallingServerResult.UserNameIsInUse}"))
                    {
                        var popupOut = await _modalPopup.GenericPopupAsync(new GenericPopupIn
                        {
                            Title = "Error",
                            Text = $"User name {connectionRequestParameters.ConnectionParameters.UserName} " +
                                   $"is in use. Please enter another name or 'Cancel' to cancel the call.",
                            EntryPlaceholder = "New user name",
                            Ok = "OK",
                            Cancel = "Cancel"
                        });
                        await OnPageDisappearingAsync();
                        if (popupOut.Ok)
                        {
                            connectionRequestParameters.ConnectionParameters.UserName = popupOut.Entry;
                            await OnPageAppearingAsync(connectionRequestParameters.ConnectionParameters, _reRender);
                        }
                        else
                        {
                            await _navigation.NavigateToPageAsync("", "ConnectionParametersPage");
                        }
                    }
                    else 
                    {
                        var popupOut = await _modalPopup.GenericPopupAsync(new GenericPopupIn
                        {
                            Title = "Error",
                            Text = $"An error occured during the connection. Here is the reported error message:" +
                                   Environment.NewLine + 
                                   $"{exception.Message}",
                            Ok = "Try again",
                            Cancel = "Cancel"
                        });
                        Disconnect();
                        if (popupOut.Ok)
                        {
                            Connect(connectionRequestParameters);
                        }
                        else
                        {
                            await _navigation.NavigateToPageAsync("", "ConnectionParametersPage");
                        }
                    }
                },
                onCompleted: () =>
                {
                    _logger.LogInformation($"************* APP OnCompleted");
                });
        }

        void Disconnect()
        {
            _mediaRecorderManager.ResetAllAsync();
            _mediaStreamManager.Clear();
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
                _displayStream = null;
            }
            else
            {
                _displayStream ??= await _localMediaStream.GetDisplayMediaStreamAync();

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

        bool _isRecording;
        string _recordButtonText = "Start recording";
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

                await _mediaRecorderManager.StopAsync(_recordingFileName);
            }
            else
            {
                // Start recording.
                RecordButtonText = "Stop recording";

                var mediaRecorderOptions = new MediaRecorderOptions
                {
                    MimeType = "video/webm",
                };
                await _mediaRecorderManager.StartAsync(_recordingFileName, 5000, /*_displayStream*/ _cameraStream,
                    mediaRecorderOptions);

                //_mediaRecorderBlobFileStream = await _mediaRecorderFileStreamFactory
                //    .CreateBlobStreamAsync("MyFirst.webm", mediaRecorderOptions);

                //var window = WebRtcMiddleware.WebRtc.Window(_jsRuntime);

                //_mediaRecorder = window.MediaRecorder(/*_displayStream*/ _cameraStream, mediaRecorderOptions); 
                //_mediaRecorder.OnDataAvailable += async (s, e) => 
                //{
                //    var blob = e.Data;
                //    _logger.LogInformation($"---------------------------- RECORDER BLOB DATA: size:{blob.Size} type:{blob.Type }");

                //    await _mediaRecorderBlobFileStream.WriteAsync(blob);
                //};
                //_mediaRecorder.OnStart += (s, e) => 
                //{
                //    _logger.LogInformation("---------------------------- RECORDER STARTED");
                //};
                
                //_mediaRecorder.Start(5000);
            }
            _isRecording = !_isRecording;
        }
    }
}
