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
    [QueryProperty(nameof(CallParametersJson), nameof(CallParametersJson))]
    public class VideoCallViewModel : INotifyPropertyChanged, IPageLifecycle
    {
        public string CallParametersJson
        {
            set
            {
                var callParametersJson = Uri.UnescapeDataString(value);
                var callParameters = JsonSerializer.Deserialize<CallParameters>(callParametersJson);
                JoinCallRequestParameters.TurnServerName = callParameters.TurnServerName;
                JoinCallRequestParameters.RoomName = callParameters.RoomName;
                JoinCallRequestParameters.UserName = callParameters.UserName;
            }
        }
        
        private IMediaStreamService _mediaStreamService;



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task OnPageAppearing()
        {
            await App.MediaStreamService.SetCameraMediaStreamPermissionsAsync();
            LocalStream = await App.MediaStreamService.GetCameraMediaStreamAsync();
            App.MediaStreamService.SetCameraMediaStreamCapturer(LocalStream);
            JoinCallRequestParameters.LocalStream = LocalStream;
            JoinCallCommand();
        }

        public Task OnPageDisappearing()
        {
            return Task.CompletedTask;
        }


        private IMediaStream _localStream;
        public IMediaStream LocalStream
        { 
            get => _localStream; 
            set
            {
                _localStream = value;
                OnPropertyChanged();
            }
        }

        private string _localLabel;
        public string LocalLabel
        {
            get => _localLabel;
            set
            {
                _localLabel = value;
                OnPropertyChanged();
            }
        }

        private bool _localVideoMuted;
        public bool LocalVideoMuted
        {
            get => _localVideoMuted;
            set
            {
                _localVideoMuted = value;
                OnPropertyChanged();
            }
        }

        private bool _localAudioMuted;
        public bool LocalAudioMuted
        {
            get => _localAudioMuted;
            set
            {
                _localAudioMuted = value;
                OnPropertyChanged();
            }
        }

        //private bool _localIsCamera;
        //public bool LocalIsCamera
        //{
        //    get => _localIsCamera;
        //    set
        //    {
        //        _localIsCamera = value;
        //        OnPropertyChanged();
        //    }
        //}


        private IMediaStream _remote1Stream;
        public IMediaStream Remote1Stream
        {
            get => _remote1Stream;
            set
            {
                _remote1Stream = value;
                OnPropertyChanged();
            }
        }

        private string _remote1Label;
        public string Remote1Label
        {
            get => _remote1Label;
            set
            {
                _remote1Label = value;
                OnPropertyChanged();
            }
        }

        private bool _remote1VideoMuted;
        public bool Remote1VideoMuted
        {
            get => _remote1VideoMuted;
            set
            {
                _remote1VideoMuted = value;
                OnPropertyChanged();
            }
        }

        private bool _remote1AudioMuted;
        public bool Remote1AudioMuted
        {
            get => _remote1AudioMuted;
            set
            {
                _remote1AudioMuted = value;
                OnPropertyChanged();
            }
        }

        //private bool _remote1IsCamera;
        //public bool Remote1IsCamera
        //{
        //    get => _remote1IsCamera;
        //    set
        //    {
        //        _remote1IsCamera = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public string SelectedTurnServerName { get; set; }
        //// Useful during development. DELETE THIS LATER!!! WILL NOT BE VISIBLE ON SCREEN
        //= "StunOnly";


        public JoinCallRequestParameters JoinCallRequestParameters { get; set; } = new JoinCallRequestParameters()
     //// Useful during development. DELETE THIS LATER!!!
     //{ RoomName = "hello",  UserName="delya"}
            ;


        //public ICommand JoinCallCommand => new Command(async () =>
        private void JoinCallCommand()
        {

            //_roomService = await _webRtcMiddleware.CreateRoomServiceAsync(App.Configuration["SignallingServer:BaseUrl"]);
            //TurnServerNames = (await _roomService.GetTurnServerNames()).ToList();


            //            JoinCallRequestParameters.TurnServerName = SelectedTurnServerName;
            //JoinCallRequestParameters.LocalStream = LocalStream;
            var peerCallbackDisposer = App.SignallingServerService.JoinRoomRequest(JoinCallRequestParameters).Subscribe(
                onNext: (peerCallbackParameters) =>
                {
                    switch (peerCallbackParameters.Code)
                    {
                        case PeerCallbackCode.PeerJoined:
                            Remote1Stream = peerCallbackParameters.MediaStream;
                            Remote1Label = peerCallbackParameters.PeerUserName;
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
