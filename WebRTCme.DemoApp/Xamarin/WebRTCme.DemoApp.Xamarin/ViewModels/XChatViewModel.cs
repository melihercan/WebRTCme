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
using WebRTCme.DemoApp.Xamarin.Models;
using WebRTCme.Middleware;
using Xamarin.Forms;
using Xamarinme;

namespace DemoApp.ViewModels
{
    [QueryProperty(nameof(ConnectionParametersJson), nameof(ConnectionParametersJson))]
    public class XChatViewModel : INotifyPropertyChanged, IPageLifecycle
    {
        private IDisposable _connectionDisposer;
        public ObservableCollection<ChatParameters> Messages { get; set; }

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

        private string _outgoingText = string.Empty;

        public string OutgoingText
        {
            get => _outgoingText; 
            set
            {
                _outgoingText = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendCommand => new Command(async () => 
        { 
        });

        public Task OnPageAppearing()
        {
            Connect();
            return Task.CompletedTask;
        }

        public Task OnPageDisappearing()
        {
            Disconnect();
            return Task.CompletedTask;
        }

        public ConnectionRequestParameters ConnectionRequestParameters { get; set; } = new ConnectionRequestParameters();

        private void Connect()
        {
            ConnectionRequestParameters.DataChannelName = ConnectionRequestParameters.RoomName;
            _connectionDisposer = App.SignallingServerService.ConnectionRequest(ConnectionRequestParameters).Subscribe(
                onNext: (peerResponseParameters) =>
                {
                    switch (peerResponseParameters.Code)
                    {
                        case PeerResponseCode.PeerJoined:
                            if (peerResponseParameters.DataChannel != null)
                            {
                                var dataChannel = peerResponseParameters.DataChannel;
                                Console.WriteLine($"--------------- DataChannel: {dataChannel.Label} " +
                                    $"state:{dataChannel.ReadyState}");
                            }
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
