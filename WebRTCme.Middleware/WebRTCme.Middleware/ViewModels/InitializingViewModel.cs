using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Ardalis.Result;
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
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ISignallingServerService _signallingServerService;
        private readonly INavigationService _navigationService;
        private string[] _turnServerNames;

        public InitializingViewModel(ISignallingServerService signallingServerService,
            INavigationService navigationService)
        {
            _signallingServerService = signallingServerService;
            _navigationService = navigationService;
        }

        private bool _isCheckingSignallingServer;
        public bool IsCheckingSignallingServer
        {
            get => _isCheckingSignallingServer;
            set
            {
                _isCheckingSignallingServer = value;
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
            await ExecuteAsync();
        }

        public ICommand RetryCommand => new AsyncCommand(async () => await Retry());

        public async Task ExecuteAsync()
        {
            if (await GetTurnServerNamesAsync())
                await NavigateToConnectionParametersPage();
        }

        private async Task<bool> GetTurnServerNamesAsync()
        {
            try
            {
                IsCheckingSignallingServer = true;
                Header = string.Empty;
                Message = $"Getting TURN server list from signalling server...";
                RetryButtonEnabled = false;

                _turnServerNames = await _signallingServerService.GetTurnServerNames();

                IsCheckingSignallingServer = false;
                Header = string.Empty;
                Message = string.Empty;
                RetryButtonEnabled = false;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("-------- " + ex.Message);
                IsCheckingSignallingServer = false;
                Header = "TURN server list couldn't be obtained";
                Message = "Make sure signalling server is online and try again";
                RetryButtonEnabled = true;
            }
            return false;
        }


        private async Task NavigateToConnectionParametersPage()
        {
            var turnServerNamesJson = JsonSerializer.Serialize(_turnServerNames);
            await _navigationService.NavigateToPageAsync(
                "///",
                "ConnectionParametersPage",
                "TurnServerNamesJson", 
                turnServerNamesJson);
        }
    }
}
