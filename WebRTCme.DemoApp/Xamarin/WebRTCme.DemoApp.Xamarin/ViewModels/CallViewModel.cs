using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme;
using WebRTCme.DemoApp.Xamarin.Models;
using WebRTCme.Middleware;
using Xamarin.Forms;
using Xamarinme;

namespace DemoApp.ViewModels
{
    [QueryProperty(nameof(ConnectionParametersJson), nameof(ConnectionParametersJson))]
    public class CallViewModel : INotifyPropertyChanged, IPageLifecycle
    {
        public string ConnectionParametersJson
        {
            set
            {
                var connectionParametersJson = Uri.UnescapeDataString(value);
                var connectionParameters = JsonSerializer.Deserialize<ConnectionParameters>(connectionParametersJson);
                ConnectionRequestParameters.TurnServerName = connectionParameters.TurnServerName;
                ConnectionRequestParameters.RoomName = connectionParameters.RoomName;
                ConnectionRequestParameters.UserName = connectionParameters.UserName;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task OnPageAppearing()
        {
            await XamarinSupport.SetCameraAndMicPermissionsAsync();
            LocalStream = await App.MediaStreamService.GetCameraMediaStreamAsync();
            Connect();
        }

        public Task OnPageDisappearing()
        {
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

        public ConnectionRequestParameters ConnectionRequestParameters { get; set; } = new ConnectionRequestParameters()
     //// Useful during development. DELETE THIS LATER!!!
     //{ RoomName = "hello",  UserName="delya"}
            ;


        private void Connect()
        {
            ConnectionRequestParameters.LocalStream = LocalStream;
            var connectionResponseDisposer = App.SignallingServerService.ConnectionRequest(ConnectionRequestParameters)
                .Subscribe(
                    onNext: (connectionResponseParameters) =>
                    {
                        if (connectionResponseParameters.MediaStream != null)
                        {
                            Remote1Stream = connectionResponseParameters.MediaStream;
                            Remote1Label = connectionResponseParameters.PeerUserName;
                        }
                    },
                    onError: (exception) =>
                    {
                    },
                    onCompleted: () =>
                    {
                    });
        }
    }
}
