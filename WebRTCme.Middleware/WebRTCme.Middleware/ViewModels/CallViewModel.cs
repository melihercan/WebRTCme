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
using WebRTCme;

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
        private readonly ISignallingServerService _signallingServerService;
        private readonly IMediaManagerService _mediaManagerService;
        private readonly INavigationService _navigationService;
        private readonly IRunOnUiThreadService _runOnUiThreadService;
        private IDisposable _connectionDisposer;
        private Action _reRender;
        private string _userName;

        public CallViewModel(IMediaStreamService mediaStreamService, ISignallingServerService signallingServerService,
            IMediaManagerService mediaManagerService, INavigationService navigationService,
            IRunOnUiThreadService runOnUiThreadService)

        {
            _mediaStreamService = mediaStreamService;
            _signallingServerService = signallingServerService;
            _mediaManagerService = mediaManagerService;
            _navigationService = navigationService;
            _runOnUiThreadService = runOnUiThreadService;
            MediaParametersList = mediaManagerService.MediaParametersList;
        }

        public async Task OnPageAppearingAsync(ConnectionParameters connectionParameters, Action reRender = null)
        {
            _reRender = reRender;
            _userName = connectionParameters.UserName;
            var localStream = await _mediaStreamService.GetCameraMediaStreamAsync();
            _mediaManagerService.Add(new MediaParameters
            {
                Label = connectionParameters.UserName,
                Stream = localStream,
                VideoMuted = false,
                AudioMuted = false,
                ShowControls = false
            });

            reRender?.Invoke();

            var connectionRequestParameters = new ConnectionRequestParameters
            {
                ConnectionParameters = connectionParameters,
                LocalStream = localStream,
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
            _connectionDisposer = _signallingServerService.ConnectionRequest(connectionRequestParameters).Subscribe(
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

        private bool _isSharingScreen = false;
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

        public Task OnShareScreenAsync()
        {
            if (_isSharingScreen)
            {
                // Stop sharing.
                ShareScreenButtonText = "Start sharing screen";
            }
            else
            {
                // Start sharing.
                ShareScreenButtonText = "Stop sharing screen";
            }
            _isSharingScreen = !_isSharingScreen;

            return Task.CompletedTask;
        }

        public ICommand ShareScreenCommand => new AsyncCommand(async () =>
        {
            await OnShareScreenAsync();
        });
    }
}
