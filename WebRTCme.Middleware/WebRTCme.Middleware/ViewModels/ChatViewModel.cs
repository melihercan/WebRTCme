using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        // A reference is required here. otherwise binding does not work.
        public ObservableCollection<DataParameters> DataParametersList { get; set; }

        private readonly ISignallingServerService _signallingServerService;
        public readonly IDataManagerService DataManager;
        private readonly INavigationService _navigationService;
        private IDisposable _connectionDisposer;
        private Action _reRender;

        public ChatViewModel(ISignallingServerService signallingServerService, IDataManagerService dataManager, 
            INavigationService navigationService)
        {
            _signallingServerService = signallingServerService;
             DataManager = dataManager;
            DataParametersList = dataManager.DataParametersList;
            _navigationService = navigationService;
        }

        public Task OnPageAppearingAsync(ConnectionParameters connectionParameters, Action reRender = null)
        {
            _reRender = reRender;
            if (_reRender is not null)
                AddOrRemoveReRenderNotification();

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
            if (_reRender is not null)
                AddOrRemoveReRenderNotification(true);
            return Task.CompletedTask;
        }

        private void AddOrRemoveReRenderNotification(bool isRemove = false)
        {
            if (isRemove)
            {
                DataParametersList.CollectionChanged -= DataParametersList_CollectionChanged;
            }
            else
            {
                DataParametersList.CollectionChanged += DataParametersList_CollectionChanged;
            }
            
            void DataParametersList_CollectionChanged(object sender, 
                System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                _reRender();
            }
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

        public void Send()
        {
            if (!string.IsNullOrEmpty(OutgoingText) && !string.IsNullOrWhiteSpace(OutgoingText))
            {
                DataManager.SendString(OutgoingText);
                OutgoingText = string.Empty;
            }
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
