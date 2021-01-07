using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Xamarin;
using Xamarin.Forms;
using Xamarinme;

namespace DemoApp.ViewModels
{
    public class VideoMeetingViewModel : /*INotifyPropertyChanged,*/ IPageLifecycle
    {
        private IRoomService _roomService;

        //public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged([CallerMemberName] string name = null) => 
          //  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task OnPageAppearing()
        {
            WebRtcMiddleware.Initialize(App.Configuration["SignallingServer:BaseUrl"]);
            _roomService = await WebRtcMiddleware.CreateRoomServiceAsync();
        }

        public async Task OnPageDisappearing()
        {
            await _roomService.DisposeAsync();
            WebRtcMiddleware.Cleanup();
        }

        //private string _cameraSource = "Default";
        public string CameraSource { get; set; } = "Default";
        //{
          //  get => _cameraSource;
            //set
            //{
              //  _cameraSource = value;
                //OnPropertyChanged();
            //}
        //}

        public IList<string> TurnServers => Enum.GetNames(typeof(TurnServer));
        public string SelectedTurnServer { get; set; }


        //private RoomParameters _roomParameters = new RoomParameters();
        public RoomParameters RoomParameters { get; set; } = new RoomParameters();
        //{ 
          //  get => _roomParameters;
            //set
            //{
              //  _roomParameters = value;
                //OnPropertyChanged();
            //}
        //}

        public ICommand StartCallCommand => new Command(async () =>
        {
            RoomParameters.IsJoin = false;
            await ConnectRoomAsync();
        });

        public ICommand JoinCallCommand => new Command(async () =>
        {
            RoomParameters.IsJoin = true;
            await ConnectRoomAsync();
        });

        private Task ConnectRoomAsync()
        {
            RoomParameters.TurnServer = (TurnServer)Enum.Parse(typeof(TurnServer), SelectedTurnServer);
            return _roomService.ConnectRoomAsync(RoomParameters);
        }

    }
}
