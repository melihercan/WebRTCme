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
        private readonly INavigationService _navigationService;
        private readonly IMediaManagerService _mediaManagerService;
        private IDisposable _connectionDisposer;
        private Action _reRender;

        public CallViewModel(IMediaStreamService mediaStreamService, ISignallingServerService signallingServerService, 
            INavigationService navigationService, IMediaManagerService mediaManagerService)
        {
            _mediaStreamService = mediaStreamService;
            _signallingServerService = signallingServerService;
            _navigationService = navigationService;
            _mediaManagerService = mediaManagerService;
            MediaParametersList = mediaManagerService.MediaParametersList;
        }

        public async Task OnPageAppearingAsync(ConnectionParameters connectionParameters, Action reRender = null)
        {
            _reRender = reRender;
            LocalStream = await _mediaStreamService.GetCameraMediaStreamAsync();

            _mediaManagerService.AddPeer("one", new MediaParameters
            {
                Stream = LocalStream
            });
            _mediaManagerService.AddPeer("two", new MediaParameters
            {
                Stream = LocalStream
            });
            _mediaManagerService.AddPeer("three", new MediaParameters
            {
                Stream = LocalStream
            });
            _mediaManagerService.AddPeer("four", new MediaParameters
            {
                Stream = LocalStream
            });


            //LocalLabel = connectionParameters.UserName;
            //var connectionRequestParameters = new ConnectionRequestParameters
            //{
            //ConnectionParameters = connectionParameters,
            //LocalStream = LocalStream,
            //};
            //Connect(connectionRequestParameters);

        }

        public Task OnPageDisappearingAsync()
        {
            Disconnect();
            return Task.CompletedTask;
        }

        private IMediaStream _localStream;
        public IMediaStream LocalStream
        { 
            get => _localStream; 
            set
            {
                _localStream = value;
                OnPropertyChanged();
            }
        }

        private string _localLabel;
        public string LocalLabel
        {
            get => _localLabel;
            set
            {
                _localLabel = value;
                OnPropertyChanged();
            }
        }

        private bool _localVideoMuted;
        public bool LocalVideoMuted
        {
            get => _localVideoMuted;
            set
            {
                _localVideoMuted = value;
                OnPropertyChanged();
            }
        }

        private bool _localAudioMuted;
        public bool LocalAudioMuted
        {
            get => _localAudioMuted;
            set
            {
                _localAudioMuted = value;
                OnPropertyChanged();
            }
        }

        private IMediaStream _remote1Stream;
        public IMediaStream Remote1Stream
        {
            get => _remote1Stream;
            set
            {
                _remote1Stream = value;
                OnPropertyChanged();
            }
        }

        private string _remote1Label;
        public string Remote1Label
        {
            get => _remote1Label;
            set
            {
                _remote1Label = value;
                OnPropertyChanged();
            }
        }

        private bool _remote1VideoMuted;
        public bool Remote1VideoMuted
        {
            get => _remote1VideoMuted;
            set
            {
                _remote1VideoMuted = value;
                OnPropertyChanged();
            }
        }

        private bool _remote1AudioMuted;


        public bool Remote1AudioMuted
        {
            get => _remote1AudioMuted;
            set
            {
                _remote1AudioMuted = value;
                OnPropertyChanged();
            }
        }

        private void Connect(ConnectionRequestParameters connectionRequestParameters)
        {
 ////connectionRequestParameters.DataChannelName = "Testing";
            _connectionDisposer = _signallingServerService.ConnectionRequest(connectionRequestParameters).Subscribe(
                onNext: (peerResponseParameters) =>
                {
                    switch (peerResponseParameters.Code)
                    {
                        case PeerResponseCode.PeerJoined:
                            if (peerResponseParameters.MediaStream != null)
                            {
                                Remote1Stream = peerResponseParameters.MediaStream;
                                Remote1Label = peerResponseParameters.PeerUserName;

                                _reRender?.Invoke();
                            }
                            
                            //if (peerResponseParameters.DataChannel != null)
                            //{
                            //    var dataChannel = peerResponseParameters.DataChannel;
                            //    Console.WriteLine($"--------------- DataChannel: {dataChannel.Label} " +
                            //        $"state:{dataChannel.ReadyState}");
                            //}

                            break;

                        case PeerResponseCode.PeerLeft:
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerLeft");
                            break;

                        case PeerResponseCode.PeerError:
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
            _connectionDisposer.Dispose();
        }
    }
}
