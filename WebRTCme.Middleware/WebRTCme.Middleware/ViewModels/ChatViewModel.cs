//using Microsoft.AspNetCore.Components.Forms;
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
using WebRTCme;

namespace WebRTCme.Middleware
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // A reference is required here. otherwise binding does not work.
        public ObservableCollection<DataParameters> DataParametersList { get; set; }

        readonly IWebRtcConnection _webRtcConnection;
        readonly IDataManager _dataManager;
        readonly IModalPopup _modalPopup;
        IDisposable _connectionDisposer;
        Action _reRender;

        public ChatViewModel(IWebRtcConnection webRtcConnection, IDataManager dataManager, IModalPopup modalPopup)
        {
            _webRtcConnection = webRtcConnection;
            _dataManager = dataManager;
            _modalPopup = modalPopup;
            DataParametersList = dataManager.DataParametersList;
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

        private void Connect(ConnectionRequestParameters connectionRequestParameters)
        {
            _connectionDisposer = _webRtcConnection.ConnectionRequest(connectionRequestParameters).Subscribe(
                // 'async' here is fire-and-forget!!! It is OK for exceptions and error messages only.
                onNext: async peerResponseParameters =>
                {
                    switch (peerResponseParameters.Code)
                    {
                        case PeerResponseCode.PeerJoined:
                            if (peerResponseParameters.DataChannel != null)
                            {
                                var dataChannel = peerResponseParameters.DataChannel;
                                Console.WriteLine($"--------------- DataChannel: {dataChannel.Label} " +
                                    $"state:{dataChannel.ReadyState}");

                                _dataManager.AddPeer(peerResponseParameters.PeerUserName, dataChannel);
                            }
                            break;

                        case PeerResponseCode.PeerLeft:
                            _dataManager.RemovePeer(peerResponseParameters.PeerUserName);
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerLeft");
                            break;

                        case PeerResponseCode.PeerError:
                            _dataManager.RemovePeer(peerResponseParameters.PeerUserName);
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerError");

                            _ = await _modalPopup.GenericPopupAsync(new GenericPopupIn 
                            {
                                Title = "Error",
                                Text = peerResponseParameters.ErrorMessage,
                                Ok = "OK"
                            });
                            break;
                    }

                },
                onError: async exception =>
                {
                    System.Diagnostics.Debug.WriteLine($"************* APP OnError:{exception.Message}");
                    _ = await _modalPopup.GenericPopupAsync(new GenericPopupIn 
                    {
                        Title = "Error",
                        Text = exception.Message,
                        Ok = "OK"
                    });
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
