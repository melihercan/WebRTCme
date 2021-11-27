//using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme.Connection;

namespace WebRTCme.Middleware
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // A reference is required here. otherwise binding does not work.
        public ObservableCollection<DataParameters> DataParametersList { get; set; }

        readonly INavigation _navigation;
        readonly IDataManager _dataManager;
        readonly IModalPopup _modalPopup;
        readonly ILogger<ChatViewModel> _logger;
        readonly IConnectionFactory _connectionFactory;

        readonly Guid _guid = Guid.NewGuid();

        IConnection _connection;
        UserContext _userContext;

        IDisposable _connectionDisposer;
        ConnectionParameters _connectionParameters;
        Action _reRender;

        public ChatViewModel(INavigation navigation, IDataManager dataManager, IModalPopup modalPopup,
            ILogger<ChatViewModel> logger, IConnectionFactory connectionFactory)
        {
            _navigation = navigation;
            _dataManager = dataManager;
            _modalPopup = modalPopup;
            _logger = logger;
            _connectionFactory = connectionFactory;

            DataParametersList = dataManager.DataParametersList;
        }

        public Task OnPageAppearingAsync(ConnectionParameters connectionParameters, Action reRender = null)
        {
            _connectionParameters = connectionParameters;
            _reRender = reRender;
            if (_reRender is not null)
                AddOrRemoveReRenderNotification();

            _connection = _connectionFactory.SelectConnection(connectionParameters.ConnectionType);
            _userContext = new()
            {
                ConnectionType = connectionParameters.ConnectionType,
                Id = _guid,
                Name = connectionParameters.Name,
                Room = connectionParameters.Room,
                DataChannelName = connectionParameters.Room
            };

            Connect();

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

        public Task SendMessageAsync()
        {
            if (!string.IsNullOrEmpty(OutgoingText) && !string.IsNullOrWhiteSpace(OutgoingText))
            {
                _dataManager.SendMessage(new Message { Text = OutgoingText });
                OutgoingText = string.Empty;
            }

            return Task.CompletedTask;
        }

        public ICommand SendMessageCommand => new AsyncCommand(async () => 
        {
            await SendMessageAsync();
        });

        public void SendMessage()
        {
        }

        public Task SendFileAsync(File file, Stream stream)
        {
            return _dataManager.SendFileAsync(file, stream);
        }

        public void SendLink()
        {
        }

        private void Connect()
        {
            _connectionDisposer = _connection.ConnectionRequest(_userContext).Subscribe(
                // 'async' here is fire-and-forget!!! It is OK for exceptions and error messages only.
                onNext: async peerResponse =>
                {
                    switch (peerResponse.Type)
                    {
                        case PeerResponseType.PeerJoined:
                            if (peerResponse.DataChannel != null)
                            {
                                var dataChannel = peerResponse.DataChannel;
                                Console.WriteLine($"--------------- DataChannel: {dataChannel.Label} " +
                                    $"state:{dataChannel.ReadyState}");

                                _dataManager.AddPeer(peerResponse.Name, dataChannel);
                            }
                            //else
                            //{
                            //    var producerDataChannel = peerResponse.ProducerDataChannel;
                            //    var consumerDataChannel = peerResponse.ConsumerDataChannel;
                            //    Console.WriteLine($"--------------- ProducerDataChannel: {producerDataChannel.Label} " +
                            //        $"state:{producerDataChannel.ReadyState}");

                            //    _dataManager.AddPeer(peerResponse.Name, null, producerDataChannel, consumerDataChannel);
                            //}
                            break;

                        case PeerResponseType.PeerLeft:
                            _dataManager.RemovePeer(peerResponse.Name);
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerLeft");
                            break;

                        case PeerResponseType.PeerError:
                            _dataManager.RemovePeer(peerResponse.Name);
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerError");

                            _ = await _modalPopup.GenericPopupAsync(new GenericPopupIn 
                            {
                                Title = "Error",
                                Text = peerResponse.ErrorMessage,
                                Ok = "OK"
                            });
                            break;
                        case PeerResponseType.PeerMedia:
                            // Nothing to do on chat.
                            _logger.LogInformation($"************* APP PeerMedia");
                            break;

                        case PeerResponseType.ProducerDataChannel:
                            _dataManager.AddPeer(peerResponse.Name, null, peerResponse.ProducerDataChannel, null);
                            break;

                        case PeerResponseType.ConsumerDataChannel:
                            _dataManager.AddPeer(peerResponse.Name, null, null, peerResponse.ConsumerDataChannel);
                            break;

                    }

                },
                onError: async exception =>
                {
                    if (exception.Message.Contains("has already joined"))
                    {
                        var popupOut = await _modalPopup.GenericPopupAsync(new GenericPopupIn
                        {
                            Title = "Error",
                            Text = $"User name {_userContext.Name} " +
                                   $"is in use. Please enter another name or 'Cancel' to cancel the call.",
                            EntryPlaceholder = "New user name",
                            Ok = "OK",
                            Cancel = "Cancel"
                        });
                        await OnPageDisappearingAsync();
                        if (popupOut.Ok)
                        {
                            _userContext.Name = popupOut.Entry;
                            _connectionParameters.Name = popupOut.Entry;
                            await OnPageAppearingAsync(_connectionParameters, _reRender);
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
                            Connect();
                        }
                        else
                        {
                            await _navigation.NavigateToPageAsync("///", "ConnectionParametersPage");
                        }
                    }
                },
                onCompleted: () =>
                {
                    System.Diagnostics.Debug.WriteLine($"************* APP OnCompleted");
                });

        }


        private void Disconnect()
        {
            _dataManager.ClearPeers();
            _connectionDisposer.Dispose();
        }
    }
}
