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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme.Connection;

namespace WebRTCme.Middleware
{
    public class CallViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // A reference is required here. otherwise binding does not work.
        public ObservableCollection<MediaStreamParameters> MediaStreamParametersList { get; set; }

        readonly INavigation _navigation;
        readonly IMediaStreamManager _mediaStreamManager;
        readonly ILocalMediaStream _localMediaStream;
        readonly IMediaRecorderManager _mediaRecorderManager;
        readonly IModalPopup _modalPopup;
        readonly IRunOnUiThread _runOnUiThread;
        readonly ILogger<CallViewModel> _logger;
        readonly IConnectionFactory _connectionFactory;

        readonly Guid _guid = Guid.NewGuid();

        IConnection _connection;
        UserContext _userContext;

        IDisposable _connectionDisposer;
        Action _reRender;
        IMediaStream _cameraStream;
        IMediaStream _displayStream;
        ConnectionParameters _connectionParameters;

        string _recordingFileName = "WebRTCme.webm";

        /// <summary>
        /// Polls GetStats for one peer and writes a summary where it can be read afterwards.
        /// </summary>
        /// <remarks>
        /// The file matters as much as the logging: on Mac Catalyst the app is sandboxed and its
        /// Debug output does not reach the unified log, so a file inside the container is the only
        /// way to see what a released build actually reported.
        /// </remarks>
        void StartStatsPolling(Guid peerId, string peerName)
        {
            StopStatsPolling(peerId);

            var cts = new CancellationTokenSource();
            _statsPollers[peerId] = cts;

            _ = Task.Run(async () =>
            {
                var path = Path.Combine(Path.GetTempPath(), "webrtcme-stats.log");

                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), cts.Token).ConfigureAwait(false);

                        var report = await _connection.GetStats(peerId).ConfigureAwait(false);

                        // Counters first, then the pair that is actually carrying the call: enough
                        // to tell a connected call from a negotiated one that never flowed.
                        var outbound = report.Values.Where(s => s.Type == "outbound-rtp").ToArray();
                        var inbound = report.Values.Where(s => s.Type == "inbound-rtp").ToArray();
                        var selected = report.Values.FirstOrDefault(s =>
                            s.Type == "candidate-pair" &&
                            s.Members.TryGetValue("state", out var state) && (string)state == "succeeded");

                        var line =
                            $"{DateTime.Now:HH:mm:ss} {peerName} entries:{report.Count} " +
                            $"out:[{string.Join(" ", outbound.Select(Describe))}] " +
                            $"in:[{string.Join(" ", inbound.Select(Describe))}] " +
                            $"pair:{(selected is null ? "none" : Member(selected, "bytesSent") + "/" + Member(selected, "bytesReceived"))}";

                        _logger.LogInformation($"************* STATS {line}");
                        System.Diagnostics.Debug.WriteLine($"######## STATS {line}");
                        System.IO.File.AppendAllText(path, line + Environment.NewLine);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        var line = $"{DateTime.Now:HH:mm:ss} {peerName} FAILED: {exception.GetType().Name}: {exception.Message}";
                        _logger.LogInformation($"************* STATS {line}");
                        System.Diagnostics.Debug.WriteLine($"######## STATS {line}");
                        try { System.IO.File.AppendAllText(path, line + Environment.NewLine); } catch { }
                        return;
                    }
                }
            });
        }

        static string Describe(RTCStats stats) =>
            $"{Member(stats, "kind")}:{Member(stats, "bytesSent")}{Member(stats, "bytesReceived")}";

        static string Member(RTCStats stats, string name) =>
            stats.Members.TryGetValue(name, out var value) ? value?.ToString() ?? string.Empty : string.Empty;

        void StopStatsPolling(Guid peerId)
        {
            if (_statsPollers.Remove(peerId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }


        // One poller per connected peer, so a peer leaving stops only its own.
        readonly Dictionary<Guid, CancellationTokenSource> _statsPollers = new();

        public CallViewModel(INavigation navigation, ILocalMediaStream localMediaStream, 
            IMediaStreamManager mediaStreamManager,
            IMediaRecorderManager mediaRecorderManager,
            IModalPopup modalPopup, 
            IRunOnUiThread runOnUiThreadService, ILogger<CallViewModel> logger, IConnectionFactory connectionFactory)
        {
            _navigation = navigation;
            _localMediaStream = localMediaStream;
            _mediaStreamManager = mediaStreamManager;
            _mediaRecorderManager = mediaRecorderManager;
            _modalPopup = modalPopup;
            _runOnUiThread = runOnUiThreadService;
            _logger = logger;
            _connectionFactory = connectionFactory;

            MediaStreamParametersList = mediaStreamManager.MediaStreamParametersList;
        }

        public async Task OnPageAppearingAsync(ConnectionParameters connectionParameters, Action reRender = null)
        {
            _connectionParameters = connectionParameters;
            _reRender = reRender;
            _cameraStream = await _localMediaStream.GetCameraMediaStreamAsync();
            _mediaStreamManager.Add(new MediaStreamParameters
            {
                Stream = _cameraStream,
                Label = connectionParameters.Name,
                Hangup = false,
                VideoMuted = false,
                AudioMuted = true,  // prevents local echo
                CameraType = CameraType.Default,
                ShowControls = false
            });

            reRender?.Invoke();

            _connection = _connectionFactory.SelectConnection(connectionParameters.ConnectionType);
            _userContext = new() 
            { 
                ConnectionType = connectionParameters.ConnectionType,
                Id = _guid,
                Name = connectionParameters.Name,
                Room = connectionParameters.Room,
                LocalStream = _cameraStream
            };

            Connect();
        }

        public Task OnPageDisappearingAsync()
        {
            Disconnect();
            return Task.CompletedTask;
        }


        void Connect()
        {
            // ConnectionRequest is a cold observable: every subscription joins the room. Dropping
            // any live one first keeps a second Connect from leaving two joins outstanding.
            _connectionDisposer?.Dispose();

            _connectionDisposer = _connection.ConnectionRequest(_userContext).Subscribe(
                // 'async' here is fire-and-forget!!! It is OK for exceptions and error messages only.
                onNext: async peerResponse =>
                {
                    switch (peerResponse.Type)
                    {
                        case PeerResponseType.PeerJoined:
                            if (peerResponse.MediaStream != null)
                            {
                                _runOnUiThread.Invoke((Action)(() =>
                                {
                                    _mediaStreamManager.Add((MediaStreamParameters)new MediaStreamParameters
                                    {
                                        Stream = peerResponse.MediaStream,
                                        Label = peerResponse.Name,
                                        Hangup = false,
                                        VideoMuted = false,
                                        AudioMuted = false,
                                        CameraType = CameraType.Default,
                                        ShowControls = false
                                    });

                                    //// TESTING
                                    //var first = _mediaStreamManager.MediaStreamParametersList[0];
                                    //_mediaStreamManager.Remove("Android");
                                    //_mediaStreamManager.Remove("Blazor");
                                    //_mediaStreamManager.Add(first);
                                }));

                                _reRender?.Invoke();
                            }
                            StartStatsPolling(peerResponse.Id, peerResponse.Name);
                            break;

                        case PeerResponseType.PeerLeft:
                            StopStatsPolling(peerResponse.Id);
                            _runOnUiThread.Invoke(() =>
                            {
                                _mediaStreamManager.Remove(peerResponse.Name);
                            });
                            _reRender?.Invoke();
                            _logger.LogInformation($"************* APP PeerLeft");
                            break;

                        case PeerResponseType.PeerError:
                            StopStatsPolling(peerResponse.Id);
                            _runOnUiThread.Invoke(() =>
                            {
                                _mediaStreamManager.Remove(peerResponse.Name);
                            });
                            _reRender?.Invoke();

                            _logger.LogInformation($"************* APP PeerError");
                            _ = await _modalPopup.GenericPopupAsync(new GenericPopupIn
                            {
                                Title = "Error",
                                Text = $"Peer {peerResponse.Name} indicated an error:" +
                                       Environment.NewLine +
                                       peerResponse.ErrorMessage,
                                Ok = "Ok",
                            });
                            break;
                        case PeerResponseType.PeerMedia:
                            _logger.LogInformation($"TODO: ************* APP PeerMedia");
                            break;
                    }
                },
                onError: async exception =>
                {
                    _logger.LogInformation($"************* APP OnError:{exception.Message}");
                    if (exception.Message.Contains("has already joined"))
                    {
                        var popupOut = await _modalPopup.GenericPopupAsync(new GenericPopupIn
                        {
                            Title = "Error",
                            Text = $"User name {_userContext.Name} " +
                                   $"is in use. Please enter another name or 'Cancel' to cancel the call.",
                            EntryPlaceholder = "New user name",
                            Ok = "OK",
                            Cancel = "Cancel"
                        });
                        await OnPageDisappearingAsync();
                        if (popupOut.Ok)
                        {
                            _userContext.Name = popupOut.Entry;
                            _connectionParameters.Name = popupOut.Entry;
                            await OnPageAppearingAsync(_connectionParameters, _reRender);
                        }
                        else
                        {
                            await _navigation.NavigateToPageAsync("///", "ConnectionParametersPage");
                        }
                    }
                    else
                    {
                        var popupOut = await _modalPopup.GenericPopupAsync(new GenericPopupIn
                        {
                            Title = "Error",
                            Text = $"An error occured during the connection. Here is the reported error message:" +
                                   Environment.NewLine +
                                   $"{exception.Message}",
                            Ok = "Try again",
                            Cancel = "Cancel"
                        });
                        Disconnect();
                        if (popupOut.Ok)
                        {
                            Connect();
                        }
                        else
                        {
                            await _navigation.NavigateToPageAsync("///", "ConnectionParametersPage");
                        }
                    }
                },
                onCompleted: () =>
                {
                    _logger.LogInformation($"************* APP OnCompleted");
                });
        }

        void Disconnect()
        {
            _mediaRecorderManager.ResetAllAsync();
            _mediaStreamManager.Clear();
            foreach (var id in _statsPollers.Keys.ToArray())
                StopStatsPolling(id);

            _connectionDisposer?.Dispose();
            _connectionDisposer = null;
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
                await _connection.ReplaceOutgoingTrackAsync(_displayStream.GetVideoTracks()[0],
                    _cameraStream.GetVideoTracks()[0]);
                _displayStream = null;
            }
            else
            {
                _displayStream ??= await _localMediaStream.GetDisplayMediaStreamAync();

                // Start sharing.
                ShareScreenButtonText = "Stop sharing screen";
                await _connection.ReplaceOutgoingTrackAsync(_cameraStream.GetVideoTracks()[0],
                    _displayStream.GetVideoTracks()[0]);
            }
            _isSharingScreen = !_isSharingScreen;

        }

        public ICommand ShareScreenCommand => new AsyncCommand(async () =>
        {
            await OnShareScreenAsync();
        });

        bool _isRecording;
        string _recordButtonText = "Start recording";
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

                await _mediaRecorderManager.StopAsync(_recordingFileName);
            }
            else
            {
                // Start recording.
                RecordButtonText = "Stop recording";

                var mediaRecorderOptions = new MediaRecorderOptions
                {
                    MimeType = "video/webm",
                };
                await _mediaRecorderManager.StartAsync(_recordingFileName, 5000, /*_displayStream*/ _cameraStream,
                    mediaRecorderOptions);
            }
            _isRecording = !_isRecording;
        }
    }
}
