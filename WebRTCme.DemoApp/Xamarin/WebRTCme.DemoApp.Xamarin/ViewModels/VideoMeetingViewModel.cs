using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme;
using WebRTCme.Middleware;
using Xamarin.Forms;
using Xamarinme;

namespace DemoApp.ViewModels
{
    public class VideoMeetingViewModel : INotifyPropertyChanged, IPageLifecycle
    {
        private IWebRtcMiddleware _webRtcMiddleware;
        private IRoomService _roomService;
        private IMediaStreamService _mediaStreamService;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task OnPageAppearing()
        {
            _webRtcMiddleware = CrossWebRtcMiddleware.Current;// .Initialize(App.Configuration["SignallingServer:BaseUrl"]);
            _mediaStreamService = await _webRtcMiddleware.CreateMediaStreamServiceAsync();
            LocalStream = await _mediaStreamService.GetCameraStreamAsync(LocalSource);

            _roomService = await _webRtcMiddleware.CreateRoomServiceAsync(App.Configuration["SignallingServer:BaseUrl"]);
            TurnServerNames = (await _roomService.GetTurnServerNames()).ToList();
        }

        public async Task OnPageDisappearing()
        {
            await _roomService.DisposeAsync();
            _webRtcMiddleware.Dispose();

            //WebRtcMiddleware.Cleanup();
        }

        private VideoType _localType = VideoType.Camera;
        public VideoType LocalType 
        {
            get => _localType;
            set
            {
                _localType = value;
                OnPropertyChanged();
            }
        }

        private string _localSource = "Default";
        public string LocalSource
        {
            get => _localSource;
            set
            {
                _localSource = value;
                OnPropertyChanged();
            }
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


        private VideoType _remote1Type = VideoType.Room;
        public VideoType Remote1Type
        {
            get => _remote1Type;
            set
            {
                _remote1Type = value;
                OnPropertyChanged();
            }
        }

        private string _remote1Source;
        public string Remote1Source
        {
            get => _remote1Source;
            set
            {
                _remote1Source = value;
                OnPropertyChanged();
            }
        }

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


        private List<string> _turnServerNames;
        public List<string> TurnServerNames
        {
            get => _turnServerNames;
            set
            {
                _turnServerNames = value;
                OnPropertyChanged();
            }

        }

        public string SelectedTurnServerName { get; set; }
    //// Useful during development. DELETE THIS LATER!!! WILL NOT BE VISIBLE ON SCREEN
    = "StunOnly";
        

        public JoinRoomRequestParameters JoinRoomRequestParameters { get; set; } = new JoinRoomRequestParameters()
     //// Useful during development. DELETE THIS LATER!!!
     { RoomName = "hello",  UserName="delya"}
            ;


        public ICommand JoinCallCommand => new Command( () =>
        {
            JoinRoomRequestParameters.TurnServerName = SelectedTurnServerName;
            JoinRoomRequestParameters.LocalStream = LocalStream;
            var roomCallbackDisposer = _roomService.JoinRoomRequest(JoinRoomRequestParameters).Subscribe(
                onNext: (roomCallbackParameters) =>
                {
                },
                onError: (exception) =>
                {
                },
                onCompleted: () =>
                {
                });
        });

    }
}
