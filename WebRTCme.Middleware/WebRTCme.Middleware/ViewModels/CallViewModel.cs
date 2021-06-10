using Microsoft.JSInterop;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly INavigationService _navigationService;
        private readonly IRunOnUiThreadService _runOnUiThreadService;
        private readonly IJSRuntime _jsRuntime;

        private IDisposable _connectionDisposer;
        private Action _reRender;
        private string _userName;
        private IMediaStream _cameraStream;
        private IMediaStream _displayStream;
        private ConnectionParameters _connectionParameters;

        private IMediaRecorder _mediaRecorder;

        public CallViewModel(IMediaStreamService mediaStreamService, IWebRtcConnection webRtcConnection,
            ISignallingServerService signallingServerService,
            IMediaManagerService mediaManagerService, INavigationService navigationService,
            IRunOnUiThreadService runOnUiThreadService, IJSRuntime jsRuntime = null)

        {
            _mediaStreamService = mediaStreamService;
            _webRtcConnection = webRtcConnection;
            _signallingServerService = signallingServerService;
            _mediaManagerService = mediaManagerService;
            _navigationService = navigationService;
            _runOnUiThreadService = runOnUiThreadService;
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
                await _webRtcConnection.ReplaceOutgoingVideoTracksAsync(_connectionParameters.TurnServerName,
                    _connectionParameters.RoomName, _cameraStream.GetVideoTracks()[0]);
                ShareScreenButtonText = "Start sharing screen";

            }
            else
            {
                // Start sharing.
                await _webRtcConnection.ReplaceOutgoingVideoTracksAsync(_connectionParameters.TurnServerName,
                    _connectionParameters.RoomName, _displayStream.GetVideoTracks()[0]);
                ShareScreenButtonText = "Stop sharing screen";
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

        public Task OnRecordAsync()
        {
            if (_isRecording)
            {
                // Stop recording.
                RecordButtonText = "Stop recording";
            }
            else
            {
                // Start recording.
                var window = WebRtcMiddleware.WebRtc.Window(_jsRuntime);

                _mediaRecorder = window.MediaRecorder(_displayStream);
                RecordButtonText = "Start recording";
            }
            _isRecording = !_isRecording;

            return Task.CompletedTask;
        }
    }
}
