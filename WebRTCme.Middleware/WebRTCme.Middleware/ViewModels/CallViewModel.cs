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
        readonly IMediaServerConnection _mediaServerConnection;
        readonly IMediaStreamManager _mediaStreamManager;
        readonly IMediaRecorderManager _mediaRecorderManager;
        readonly IModalPopup _modalPopup;
        readonly IRunOnUiThread _runOnUiThread;
        readonly ILogger<CallViewModel> _logger;

        IDisposable _connectionDisposer;
        Action _reRender;
        //string _userName;
        IMediaStream _cameraStream;
        IMediaStream _displayStream;
        ConnectionParameters _connectionParameters;

        string _recordingFileName = "WebRTCme.webm";

        public CallViewModel(INavigation navigation, ILocalMediaStream localMediaStream, 
            IWebRtcConnection webRtcConnection, IMediaServerConnection mediaServerConnection,
            IMediaStreamManager mediaStreamManager,
            IMediaRecorderManager mediaRecorderManager,
            IModalPopup modalPopup, 
            IRunOnUiThread runOnUiThreadService, ILogger<CallViewModel> logger)
        {
            _navigation = navigation;
            _localMediaStream = localMediaStream;
            _webRtcConnection = webRtcConnection;
            _mediaServerConnection = mediaServerConnection;
            _mediaStreamManager = mediaStreamManager;
            _mediaRecorderManager = mediaRecorderManager;
            _modalPopup = modalPopup;
            _runOnUiThread = runOnUiThreadService;
            _logger = logger;

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
                Stream = _cameraStream,
                Label = connectionParameters.UserName,
                Hangup = false,
                VideoMuted = false,
                AudioMuted = true,  // prevents local echo
                CameraType = CameraType.Default,
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
            IObservable<PeerResponseParameters> connectionObservable = null;

            if (!string.IsNullOrEmpty(connectionRequestParameters.ConnectionParameters.TurnServerName))
            {
                connectionObservable = _webRtcConnection.ConnectionRequest(connectionRequestParameters);
            }
            else if (!string.IsNullOrEmpty(connectionRequestParameters.ConnectionParameters.MediaServerName))
            {
                connectionObservable = _mediaServerConnection.ConnectionRequest(connectionRequestParameters);
            }
            else
                throw new Exception("Either TURN or Media Server should be provided");


            _connectionDisposer = connectionObservable.Subscribe(
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
                                        Stream = peerResponseParameters.MediaStream,
                                        Label = peerResponseParameters.PeerUserName,
                                        Hangup = false,
                                        VideoMuted = false,
                                        AudioMuted = false,
                                        CameraType = CameraType.Default,
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
                            await _navigation.NavigateToPageAsync("///", "ConnectionParametersPage");
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
                            await _navigation.NavigateToPageAsync("///", "ConnectionParametersPage");
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
            }
            _isRecording = !_isRecording;
        }
    }
}
