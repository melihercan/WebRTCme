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
    public class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ObservableCollection<DataParameters> DataParametersList { get; set; } = new();

        private readonly ISignallingServerService _signallingServerService;
        public readonly IDataManager DataManager;
        private readonly INavigationService _navigationService;
        private IDisposable _connectionDisposer;

        public ChatViewModel(ISignallingServerService signallingServerService, IDataManager dataManager, 
            INavigationService navigationService)
        {
            _signallingServerService = signallingServerService;
             DataManager = dataManager;
            _navigationService = navigationService;
        }

        public Task OnPageAppearingAsync(ConnectionParameters connectionParameters)
        {
            var connectionRequestParameters = new ConnectionRequestParameters
            {
                ConnectionParameters = connectionParameters,
                DataChannelName = connectionParameters.RoomName
            };
            Connect(connectionRequestParameters);
            return Task.CompletedTask;
        }

        public Task OnPageDisappearingAsync()
        {
            Disconnect();
            return Task.CompletedTask;
        }

        public ConnectionParameters ConnectionParameters { get; set; }


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

        private void Send()
        {
            DataManager.SendString(OutgoingText);
        }

        public ICommand SendCommand => new AsyncCommand(() => 
        {
            Send();
            return Task.CompletedTask;
        });


        private void Connect(ConnectionRequestParameters connectionRequestParameters)
        {
            _connectionDisposer = _signallingServerService.ConnectionRequest(connectionRequestParameters).Subscribe(
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


                                DataParametersList.Add(new DataParameters
                                {
                                    From = DataFromType.System,
                                    PeerUserName = string.Empty,
                                    PeerUserNameTextColor = "#000000",
                                    Time = DateTime.Now.ToString("HH:mm"),
                                    Message = "Hade ya"
                                });


                                DataManager.AddPeer(peerResponseParameters.PeerUserName, dataChannel);
                            }
                            break;

                        case PeerResponseCode.PeerLeft:
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerLeft");
                            DataManager.RemovePeer(peerResponseParameters.PeerUserName);
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
