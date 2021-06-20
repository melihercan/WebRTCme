using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme.Middleware;

namespace WebRTCme.Middleware
{
    public class CallViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // A reference is required here. otherwise binding does not work.
        public ObservableCollection<MediaParameters> MediaParametersList { get; set; }

        readonly ILocalMediaStream _localMediaStream;
        readonly IWebRtcConnection _webRtcConnection;
        readonly IMediaManager _mediaManager;
        readonly IVideoRecorderFileStreamFactory _videoRecorderFileStreamFactory;
        readonly IModalPopup _modalPopup;
        readonly IRunOnUiThread _runOnUiThread;
        readonly ILogger<CallViewModel> _logger;
        readonly IJSRuntime _jsRuntime;

        IDisposable _connectionDisposer;
        Action _reRender;
        string _userName;
        IMediaStream _cameraStream;
        IMediaStream _displayStream;
        ConnectionParameters _connectionParameters;

        IMediaRecorder _mediaRecorder;

        BlobStream _videoRecorderBlobFileStream;


        public CallViewModel(ILocalMediaStream localMediaStream, IWebRtcConnection webRtcConnection,
            IMediaManager mediaManager,
            IVideoRecorderFileStreamFactory videoRecorderFileStreamFactory, IModalPopup modalPopup, 
            IRunOnUiThread runOnUiThreadService, ILogger<CallViewModel> logger, 
            IJSRuntime jsRuntime = null)
        {
            _localMediaStream = localMediaStream;
            _webRtcConnection = webRtcConnection;
            _mediaManager = mediaManager;
            _videoRecorderFileStreamFactory = videoRecorderFileStreamFactory;
            _modalPopup = modalPopup;
            _runOnUiThread = runOnUiThreadService;
            _logger = logger;
            _jsRuntime = jsRuntime;
            MediaParametersList = mediaManager.MediaParametersList;
        }

        public async Task OnPageAppearingAsync(ConnectionParameters connectionParameters, Action reRender = null)
        {
            _connectionParameters = connectionParameters;
            _reRender = reRender;
            _userName = connectionParameters.UserName;
            _cameraStream = await _localMediaStream.GetCameraMediaStreamAsync();
            _mediaManager.Add(new MediaParameters
            {
                Label = connectionParameters.UserName,
                Stream = _cameraStream,
                VideoMuted = false,
                AudioMuted = false,
                ShowControls = false
            });

            reRender?.Invoke();

            var connectionRequestParameters = new ConnectionRequestParameters
            {
                ConnectionParameters = connectionParameters,
                LocalStream = _cameraStream,
            };
            Connect(connectionRequestParameters);
        }

        public Task OnPageDisappearingAsync()
        {
            Disconnect();
            return Task.CompletedTask;
        }

        private void Connect(ConnectionRequestParameters connectionRequestParameters)
        {
            _connectionDisposer = _webRtcConnection.ConnectionRequest(connectionRequestParameters).Subscribe(
                // 'async' here is fire-and-forget!!! It is OK for exceptions and error messages only.
                onNext: async peerResponseParameters =>
                {
                    switch (peerResponseParameters.Code)
                    {
                        case PeerResponseCode.PeerJoined:
                            if (peerResponseParameters.MediaStream != null)
                            {
                                _runOnUiThread.Invoke(() =>
                                {
                                    _mediaManager.Add(new MediaParameters
                                    {
                                        Label = peerResponseParameters.PeerUserName,
                                        Stream = peerResponseParameters.MediaStream,
                                        VideoMuted = false,
                                        AudioMuted = false,
                                        ShowControls = false
                                    });
                                });

                                _reRender?.Invoke();
                            }
                            break;

                        case PeerResponseCode.PeerLeft:
                            _runOnUiThread.Invoke(() =>
                            {
                                _mediaManager.Remove(peerResponseParameters.PeerUserName);
                            });
                            _reRender?.Invoke();
                            System.Diagnostics.Debug.WriteLine($"************* APP PeerLeft");
                            break;

                        case PeerResponseCode.PeerError:
                            _runOnUiThread.Invoke(() =>
                            {
                                _mediaManager.Remove(peerResponseParameters.PeerUserName);
                            });
                            _reRender?.Invoke();

                            _ = await _modalPopup.GenericPopupAsync(new GenericPopupIn 
                            { 
                                Title = "Error",
                                Text = peerResponseParameters.ErrorMessage,
                                Ok = "OK"
                            });
                            //// TODO: ADD POPUP ERROR MESSAGE

                            System.Diagnostics.Debug.WriteLine($"************* APP PeerError");
                            break;
                    }
                },
                onError: async exception =>
                {
                    System.Diagnostics.Debug.WriteLine($"************* APP OnError:{exception.Message}");
                    var popupOut = await _modalPopup.GenericPopupAsync(new GenericPopupIn 
                    {
                        Title = "Error",
                        Text = exception.Message,
                   ////EntryPlaceholder = "Re-enter user name",
                        Ok = "OK",
                        ////Cancel = "Cancel"
                    });


                },
                onCompleted: () =>
                {
                    System.Diagnostics.Debug.WriteLine($"************* APP OnCompleted");
                });
        }

        private void Disconnect()
        {
            _mediaManager.Clear();
            _connectionDisposer.Dispose();
        }

        private bool _isSharingScreen;
        private string _shareScreenButtonText = "Start sharing screen";
        public string ShareScreenButtonText
        {
            get => _shareScreenButtonText;
            set
            {
                _shareScreenButtonText = value;
                OnPropertyChanged();
            }
        }

        public async Task OnShareScreenAsync()
        {
            if (_isSharingScreen)
            {
                // Stop sharing.
                ShareScreenButtonText = "Start sharing screen";
                await _webRtcConnection.ReplaceOutgoingVideoTracksAsync(_connectionParameters.TurnServerName,
                    _connectionParameters.RoomName, _cameraStream.GetVideoTracks()[0]);
                _displayStream = null;
            }
            else
            {
                _displayStream ??= await _localMediaStream.GetDisplayMediaStreamAync();

                // Start sharing.
                ShareScreenButtonText = "Stop sharing screen";
                await _webRtcConnection.ReplaceOutgoingVideoTracksAsync(_connectionParameters.TurnServerName,
                    _connectionParameters.RoomName, _displayStream.GetVideoTracks()[0]);
            }
            _isSharingScreen = !_isSharingScreen;

        }

        public ICommand ShareScreenCommand => new AsyncCommand(async () =>
        {
            await OnShareScreenAsync();
        });

        private bool _isRecording;
        private string _recordButtonText = "Start recording";
        public string RecordButtonText
        {
            get => _recordButtonText;
            set
            {
                _recordButtonText = value;
                OnPropertyChanged();
            }
        }

        public async Task OnRecordAsync()
        {
            if (_isRecording)
            {
                // Stop recording.
                RecordButtonText = "Start recording";

                _videoRecorderBlobFileStream.Close();
                _mediaRecorder.Stop();
                _mediaRecorder.Dispose();
            }
            else
            {
                // Start recording.
                RecordButtonText = "Stop recording";

                var mediaRecorderOptions = new MediaRecorderOptions
                {
                    MimeType = "video/webm",
                };

                _videoRecorderBlobFileStream = await _videoRecorderFileStreamFactory
                    .CreateBlobStreamAsync("MyFirst.webm", mediaRecorderOptions);

                var window = WebRtcMiddleware.WebRtc.Window(_jsRuntime);

                _mediaRecorder = window.MediaRecorder(/*_displayStream*/ _cameraStream, mediaRecorderOptions); 
                _mediaRecorder.OnDataAvailable += async (s, e) => 
                {
                    var blob = e.Data;
                    _logger.LogInformation($"---------------------------- RECORDER BLOB DATA: size:{blob.Size} type:{blob.Type }");

                    await _videoRecorderBlobFileStream.WriteAsync(blob);
                };
                _mediaRecorder.OnStart += (s, e) => 
                {
                    _logger.LogInformation("---------------------------- RECORDER STARTED");
                };
                
                _mediaRecorder.Start(5000);
            }
            _isRecording = !_isRecording;
        }
    }
}
