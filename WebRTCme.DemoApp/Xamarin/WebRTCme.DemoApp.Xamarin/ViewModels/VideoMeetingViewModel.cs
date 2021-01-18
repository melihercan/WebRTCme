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
            Stream = await _mediaStreamService.GetCameraStreamAsync(Source);

            _roomService = await _webRtcMiddleware.CreateRoomServiceAsync(App.Configuration["SignallingServer:BaseUrl"]);
        }

        public async Task OnPageDisappearing()
        {
            await _roomService.DisposeAsync();
            _webRtcMiddleware.Dispose();

            //WebRtcMiddleware.Cleanup();
        }

        private VideoType _type = VideoType.Camera;
        public VideoType Type 
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        private string _source = "Default";
        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                OnPropertyChanged();
            }
        }

        private IMediaStream _stream;
        public IMediaStream Stream 
        { 
            get => _stream; 
            set
            {
                _stream = value;
                OnPropertyChanged();
            }
        }

        public IList<string> TurnServers => Enum.GetNames(typeof(TurnServer));
        public string SelectedTurnServer { get; set; }


        public RoomRequestParameters RoomRequestParameters { get; set; } = new RoomRequestParameters();

        public ICommand StartCallCommand => new Command(async () =>
        {
            RoomRequestParameters.IsInitiator = true;
            await ConnectRoomAsync();
        });

        public ICommand JoinCallCommand => new Command(async () =>
        {
            RoomRequestParameters.IsInitiator = false;
            await ConnectRoomAsync();
        });

        private Task ConnectRoomAsync()
        {
            RoomRequestParameters.TurnServer = (TurnServer)Enum.Parse(typeof(TurnServer), SelectedTurnServer);
            RoomRequestParameters.LocalStream = Stream;
            return _roomService.ConnectRoomAsync(RoomRequestParameters);
        }

    }
}
