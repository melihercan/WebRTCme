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

        readonly INavigation _navigation;
        readonly IWebRtcConnection _webRtcConnection;
        readonly IMediaServerConnection _mediaServerConnection;
        readonly IDataManager _dataManager;
        readonly IModalPopup _modalPopup;
        IDisposable _connectionDisposer;
        Action _reRender;

        public ChatViewModel(INavigation navigation, IWebRtcConnection webRtcConnection,
            IMediaServerConnection mediaServerConnection, IDataManager dataManager, IModalPopup modalPopup)
        {
            _navigation = navigation;
            _webRtcConnection = webRtcConnection;
            _mediaServerConnection = mediaServerConnection;
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
