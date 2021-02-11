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
                JoinCallRequestParameters.TurnServerName = connectionParameters.TurnServerName;
                JoinCallRequestParameters.RoomName = connectionParameters.RoomName;
                JoinCallRequestParameters.UserName = connectionParameters.UserName;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task OnPageAppearing()
        {
            JoinChatCommand();
        }

        public Task OnPageDisappearing()
        {
            return Task.CompletedTask;
        }




        public JoinCallRequestParameters JoinCallRequestParameters { get; set; } = new JoinCallRequestParameters()
     //// Useful during development. DELETE THIS LATER!!!
     //{ RoomName = "hello",  UserName="delya"}
            ;


        //public ICommand JoinCallCommand => new Command(async () =>
        private void JoinChatCommand()
        {
            var peerCallbackDisposer = App.SignallingServerService.JoinRoomRequest(JoinCallRequestParameters).Subscribe(
                onNext: (peerCallbackParameters) =>
                {
                    switch (peerCallbackParameters.Code)
                    {
                        case PeerCallbackCode.PeerJoined:
                            break;

                        case PeerCallbackCode.PeerModified:
                            break;

                        default:
                            break;
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
