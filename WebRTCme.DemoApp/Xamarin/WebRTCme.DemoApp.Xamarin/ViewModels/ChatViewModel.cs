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
using WebRTCme.DemoApp.Xamarin.Models;
using WebRTCme.Middleware;
using Xamarin.Forms;
using Xamarinme;

namespace DemoApp.ViewModels
{
    [QueryProperty(nameof(ConnectionParametersJson), nameof(ConnectionParametersJson))]
    public class ChatViewModel : INotifyPropertyChanged, IPageLifecycle
    {
        public string ConnectionParametersJson
        {
            set
            {
                var connectionParametersJson = Uri.UnescapeDataString(value);
                var connectionParameters = JsonSerializer.Deserialize<ConnectionParameters>(connectionParametersJson);
                ConnectionRequestParameters.TurnServerName = connectionParameters.TurnServerName;
                ConnectionRequestParameters.RoomName = connectionParameters.RoomName;
                ConnectionRequestParameters.UserName = connectionParameters.UserName;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Task OnPageAppearing()
        {
            Connect();
            return Task.CompletedTask;
        }

        public Task OnPageDisappearing()
        {
            return Task.CompletedTask;
        }

        public ConnectionRequestParameters ConnectionRequestParameters { get; set; } = new ConnectionRequestParameters()
     //// Useful during development. DELETE THIS LATER!!!
     //{ RoomName = "hello",  UserName="delya"}
            ;


        private void Connect()
        {
            ConnectionRequestParameters.DataChannelName = ConnectionRequestParameters.RoomName;// "hagimokkey";
            var connectionResponseDisposer = App.SignallingServerService.ConnectionRequest(ConnectionRequestParameters)
                .Subscribe(
                    onNext: (connectionResponseParameters) =>
                    {
                        if (connectionResponseParameters.DataChannel != null)
                        {
                            var dataChannel = connectionResponseParameters.DataChannel;
                            Console.WriteLine($"--------------- DataChannel: {dataChannel.Label}");
                        }
                    },
                    onError: (exception) =>
                    {
                    },
                    onCompleted: () =>
                    {
                    });
        }

    }
}
