using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using MvvmHelpers.Commands;

namespace WebRTCme.Middleware
{
    public class InitializingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null) =>
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        readonly ISignallingServer _signallingServer;
        readonly IMediaServerConnection _mediaServerConnection;
        readonly INavigation _navigation;
        
        string[] _turnServerNames;
        string[] _mediaServerNames;

        public InitializingViewModel(ISignallingServer signallingServer, IMediaServerConnection mediaServerConnection,
            INavigation navigation)
        {
            _signallingServer = signallingServer;
            _mediaServerConnection = mediaServerConnection;
            _navigation = navigation;
        }

        public async Task OnPageAppearingAsync()
        {
            if (await GetTurnOrMediaServerNamesAsync())
                await NavigateToConnectionParametersPage();
        }

        private bool _isCheckingServers;
        public bool IsCheckingServers
        {
            get => _isCheckingServers;
            set
            {
                _isCheckingServers = value;
                OnPropertyChanged();
            }
        }

        private volatile string _header;
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        private volatile string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        private bool _retryButtonEnabled;
        public bool RetryButtonEnabled
        {
            get => _retryButtonEnabled;
            set
            {
                _retryButtonEnabled = value;
                OnPropertyChanged();
            }
        }


        public async Task Retry()
        {
            await OnPageAppearingAsync();
        }

        public ICommand RetryCommand => new AsyncCommand(async () => await Retry());


        async Task<bool> GetTurnOrMediaServerNamesAsync()
        {
            try
            {
                IsCheckingServers = true;
                Header = string.Empty;
                Message = $"Getting TURN or Media server list...";
                RetryButtonEnabled = false;

                var result = await _signallingServer.GetTurnServerNamesAsync();
                if (result.Item1 == SignallingServerProxy.SignallingServerResult.Ok)
                {
                    _turnServerNames = result.Item2;
                    IsCheckingServers = false;
                    Header = string.Empty;
                    Message = string.Empty;
                    RetryButtonEnabled = false;
                    return true;
                }
                else
                {
                    throw new Exception($"{result.Item1}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("-------- " + ex.Message);
                IsCheckingServers = false;
                Header = "TURN or Media server list couldn't be obtained";
                Message = "Make sure signalling server and/or media server is online and try again";
                RetryButtonEnabled = true;
            }
            return false;
        }

        private async Task NavigateToConnectionParametersPage()
        {
            var turnServerNamesJson = JsonSerializer.Serialize(_turnServerNames);
            await _navigation.NavigateToPageAsync(
                "///",
                "ConnectionParametersPage",
                "TurnServerNamesJson", 
                turnServerNamesJson);
        }
    }
}
