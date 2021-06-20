using MvvmHelpers.Commands;
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
using WebRTCme.Middleware;
using Xamarin.Essentials;

namespace WebRTCme.Middleware
{
    public class ConnectionParametersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null) => 
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        readonly INavigation _navigation;
        
        List<string> _turnServerNames;

        public ConnectionParametersViewModel(INavigation navigation)
        {
            _navigation = navigation;

            // Default values for debugging.
            var platformName = string.Empty;
            if (DeviceInfo.Platform == DevicePlatform.Android)
                platformName = "Android";
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
                platformName = "iOS";
            else
                platformName = "Blazor";
            ConnectionParameters.TurnServerName = "StunOnly";
            ConnectionParameters.RoomName = "hello";
            ConnectionParameters.UserName = platformName;
        }

        public void OnPageAppearing(string[] turnServerNames)
        {
            if (turnServerNames is not null)
                TurnServerNames = turnServerNames.ToList();
        }

        public List<string> TurnServerNames
        {
            get => _turnServerNames;
            set
            {
                _turnServerNames = value;
                OnPropertyChanged();
            }
        }

        public ConnectionParameters ConnectionParameters { get; set; } = new ConnectionParameters();

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
            var connectionParamatersJson = JsonSerializer.Serialize(ConnectionParameters);
            await _navigation.NavigateToPageAsync(
                "",
                "ChatPage",
                "ConnectionParametersJson",
                connectionParamatersJson);
        }
    }
}
