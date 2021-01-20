using System;
using System.Collections.Generic;
using System.ComponentModel;
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


        public IList<string> TurnServers => Enum.GetNames(typeof(TurnServer));
        public string SelectedTurnServer { get; set; }


        public RoomRequestParameters RoomRequestParameters { get; set; } = new RoomRequestParameters();

        public ICommand StartCallCommand => new Command( () =>
        {
            RoomRequestParameters.IsInitiator = true;
            RoomRequest();
        });

        public ICommand JoinCallCommand => new Command( () =>
        {
            RoomRequestParameters.IsInitiator = false;
            RoomRequest();
        });

        private void RoomRequest()
        {
            RoomRequestParameters.TurnServer = (TurnServer)Enum.Parse(typeof(TurnServer), SelectedTurnServer);
            RoomRequestParameters.LocalStream = LocalStream;
            var roomCallbackDisposer = _roomService.RoomRequest(RoomRequestParameters).Subscribe(
                onNext: (roomCallbackParameters) => 
                { 
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
