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
        private ILocalMediaStreamService _localMediaStreamService;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task OnPageAppearing()
        {
#if true
            _webRtcMiddleware = CrossWebRtcMiddleware.Current;// .Initialize(App.Configuration["SignallingServer:BaseUrl"]);
            _mediaStreamService = await _webRtcMiddleware.CreateMediaStreamServiceAsync();
            LocalStream = await _mediaStreamService.GetCameraStreamAsync(LocalSource);

            _localMediaStreamService = await _webRtcMiddleware.CreateLocalMediaStreamServiceAsync();
//            LocalStream = await _localMediaStreamService.GetCameraStreamAsync();
            //LocalTrack = await _localMediaStreamService.GetCameraTrackAsync();
            //LocalView = _localMediaStreamService.GetCameraView();
            //LocalRenderer = _localMediaStreamService.GetCameraRenderer();
            //LocalCapturer = _localMediaStreamService.GetCameraCapturer();

            ////_roomService = await _webRtcMiddleware.CreateRoomServiceAsync(App.Configuration["SignallingServer:BaseUrl"]);
            ////TurnServerNames = (await _roomService.GetTurnServerNames()).ToList();
#endif            
        }

        public async Task OnPageDisappearing()
        {
            await _roomService.DisposeAsync();
            _webRtcMiddleware.Dispose();

            //WebRtcMiddleware.Cleanup();
        }

        private IMediaStreamTrack _localTrack;
        public IMediaStreamTrack LocalTrack
        {
            get => _localTrack;
            set
            {
                _localTrack = value;
                OnPropertyChanged();
            }
        }

        private IVideoView _localView;
        public IVideoView LocalView
        {
            get => _localView;
            set
            {
                _localView = value;
                OnPropertyChanged();
            }
        }

        private IVideoRenderer _localRenderer;
        public IVideoRenderer LocalRenderer
        {
            get => _localRenderer;
            set
            {
                _localRenderer = value;
                OnPropertyChanged();
            }
        }

        private IVideoCapturer _localCapturer;
        public IVideoCapturer LocalCapturer
        {
            get => _localCapturer;
            set
            {
                _localCapturer = value;
                OnPropertyChanged();
            }
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
            var peerCallbackDisposer = _roomService.JoinRoomRequest(JoinRoomRequestParameters).Subscribe(
                onNext: (peerCallbackParameters) =>
                {
                    switch (peerCallbackParameters.Code)
                    {
                        case PeerCallbackCode.PeerJoined:
                            Remote1Stream = peerCallbackParameters.MediaStream;
                            Remote1Source = "Remote";
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
        });

    }
}
