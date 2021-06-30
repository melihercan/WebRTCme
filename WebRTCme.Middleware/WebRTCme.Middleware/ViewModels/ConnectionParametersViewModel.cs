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
        readonly ISignallingServer _signallingServer;
        readonly IMediaServerConnection _mediaServerConnection;

        string[] _turnServerNames;
        string[] _mediaServerNames;

        Action _reRender;

        public ConnectionParametersViewModel(ISignallingServer signallingServer,
            IMediaServerConnection mediaServerConnection, INavigation navigation)
        {
            _signallingServer = signallingServer;
            _mediaServerConnection = mediaServerConnection;
            _navigation = navigation;

            SelectedServerType = ServerTypes[0];
            ServerNames = _turnServerNames;

            // Default values for debugging.
            var platformName = string.Empty;
            if (DeviceInfo.Platform == DevicePlatform.Android)
                platformName = "Android";
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
                platformName = "iOS";
            else
                platformName = "Blazor";
            //SelectedTurnServer = "StunOnly";
            ConnectionParameters.TurnServerName = "StunOnly";
            ConnectionParameters.RoomName = "hello";
            ConnectionParameters.UserName = platformName;
        }

        public async Task OnPageAppearingAsync(Action reRender = null/*string[] turnServerNames*/)
        {
            _reRender = reRender;
            _turnServerNames = await GetTurnServerNamesAsync();
            _mediaServerNames = await GetMediaServerNamesAsync();
            _reRender?.Invoke();


            //if (turnServerNames is not null)
            //  TurnServerNames = turnServerNames.ToList();


        }


        public readonly string[] ServerTypes = { "TURN server", "Media server" };

        string _selectedServerType;
        public string SelectedServerType
        {
            get => _selectedServerType;
            set
            {
                _selectedServerType = value;
                if (value.Equals("TURN server"))
                {
                    Task.Run(async () =>
                    {
                        _turnServerNames = await GetTurnServerNamesAsync();
                        ServerNames = _turnServerNames;
                        _reRender?.Invoke();
                    });
                }
                else if (value.Equals("Media server"))
                {
                    Task.Run(async () =>
                    {
                        _mediaServerNames = await GetMediaServerNamesAsync();
                        ServerNames = _mediaServerNames;
                        _reRender?.Invoke();
                    });
                }

            }
        }


        //string _selectedTurnServer;
        //public string SelectedTurnServer
        //{
        //    get => _selectedTurnServer;
        //    set
        //    {
        //        _selectedTurnServer = value;
        //        OnPropertyChanged();
        //    }
        //}

        string[] _serverNames;
        public string[] ServerNames
        {
            get => _serverNames;
            set
            {
                Console.WriteLine($"================== {value}");
                _serverNames = value;
                OnPropertyChanged();
            }
        }

        public string SelectedServerName { get; set; }

        //List<string> _turnServerNames;
        //public List<string> TurnServerNames
        //{
        //    get => _turnServerNames;
        //    set
        //    {
        //        _turnServerNames = value;
        //        OnPropertyChanged();
        //    }
        //}

        //List<string> _mediaServerNames;
        //public List<string> MediaServerNames
        //{
        //    get => _mediaServerNames;
        //    set
        //    {
        //        _mediaServerNames = value;
        //        OnPropertyChanged();
        //    }
        //}

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


        async Task<string[]> GetTurnServerNamesAsync()
        {
            try
            {
                var result = await _signallingServer.GetTurnServerNamesAsync();
                if (result.Item1 == SignallingServerProxy.SignallingServerResult.Ok)
                {
                    return result.Item2;
                }
                else
                {
                    throw new Exception($"{result.Item1}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("-------- " + ex.Message);
            }
            return null;
        }

        async Task<string[]> GetMediaServerNamesAsync()
        {
            try
            {
                return await _mediaServerConnection.GetMediaServerNamesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("-------- " + ex.Message);
            }
            return null;
        }


    }
}
