using Microsoft.AspNetCore.Connections;
using Microsoft.Maui.Devices;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme;
using WebRTCme.Connection;
using WebRTCme.Middleware;
////using Xamarin.Essentials;

namespace WebRTCme.Middleware
{
    public class ConnectionParametersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null) =>
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        readonly INavigation _navigation;
        readonly IModalPopup _modalPopup;
        readonly Connection.IConnectionFactory _connectionFactory;

        IConnection _connection;

        Action _reRender;

        public ConnectionParametersViewModel(INavigation navigation, IModalPopup modalPopup, 
            Connection.IConnectionFactory connectionFactory)
        {
            _navigation = navigation;
            _modalPopup = modalPopup;
            _connectionFactory = connectionFactory;

            _connection = _connectionFactory.SelectConnection(ConnectionType.MediaSoup);
            ConnectionTypeNames = Enum.GetNames(typeof(ConnectionType));

            // Default values for debugging.
            var platformName = string.Empty;
            if (DeviceInfo.Platform == DevicePlatform.Android)
                platformName = "Android";
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
                platformName = "iOS";
            else
                platformName = "Blazor";
            ConnectionParameters.Room = "hello";
            ConnectionParameters.Name = platformName;
        }

        public Task OnPageAppearingAsync(Action reRender = null/*string[] turnServerNames*/)
        {
            _reRender = reRender;
            _reRender?.Invoke();
            return Task.CompletedTask;
        }

        string[] _connectionTypeNames;
        public string[] ConnectionTypeNames
        {
            get => _connectionTypeNames;
            set
            {
                _connectionTypeNames = value;
                OnPropertyChanged();
            }
        }

        public string SelectedConnectionTypeName { get; set; }

        ConnectionParameters _connectionParameters = new();
        public ConnectionParameters ConnectionParameters 
        { 
            get => _connectionParameters;
            set 
            {
                _connectionParameters = value;
                OnPropertyChanged();
            }
        }

        public bool IsCall { get; set; }

        public async Task Join()
        {
            if (IsCall == true)
                await JoinCall();
            else
                await JoinChat();
        }

        public ICommand JoinCallCommand => new AsyncCommand(async () => await JoinCall());

        public async Task JoinCall()
        {
            if (SelectedConnectionTypeName == ConnectionType.Signaling.ToString())
            {
                ConnectionParameters.ConnectionType = ConnectionType.Signaling;
            }
            else if (SelectedConnectionTypeName == ConnectionType.MediaSoup.ToString())
            {
                ConnectionParameters.ConnectionType = ConnectionType.MediaSoup;
            }

            var connectionParamatersJson = JsonSerializer.Serialize(ConnectionParameters);
            await _navigation.NavigateToPageAsync(
                "",
                "CallPage",
                "ConnectionParametersJson",
                connectionParamatersJson);
        }

        public ICommand JoinChatCommand => new AsyncCommand(async () => await JoinChat());

        public async Task JoinChat()
        {
            if (SelectedConnectionTypeName == ConnectionType.Signaling.ToString())
            {
                ConnectionParameters.ConnectionType = ConnectionType.Signaling;
            }
            else if (SelectedConnectionTypeName == ConnectionType.MediaSoup.ToString())
            {
                ConnectionParameters.ConnectionType = ConnectionType.MediaSoup;
            }

            var connectionParamatersJson = JsonSerializer.Serialize(ConnectionParameters);
            await _navigation.NavigateToPageAsync(
                "",
                "ChatPage",
                "ConnectionParametersJson",
                connectionParamatersJson);
        }
    }
}
