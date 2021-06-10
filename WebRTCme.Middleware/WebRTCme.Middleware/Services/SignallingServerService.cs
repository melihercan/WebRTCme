using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;
using WebRTCme.SignallingServerProxy;
using WebRTCme.Middleware.Models;
using Xamarin.Essentials;

namespace WebRTCme.Middleware.Services
{
    internal class SignallingServerService : ISignallingServerService, ISignallingServerCallbacks
    {
        private readonly IWebRtcConnection _webRtcConnection;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<SignallingServerService> _logger;
        private readonly string _signallingServerBaseUrl;

        //// TODO: FIND ANOTHER SOLUTION TO PASS THIS TO WebRtcConnection.
        private readonly ISignallingServerProxy SignallingServerProxy;
//        private static List<ConnectionContext> _connectionContexts = new();

        public SignallingServerService(IWebRtcConnection webRtcConnection, IConfiguration configuration, 
            ILogger<SignallingServerService> logger,
            IJSRuntime jsRuntime = null)
        {
            _webRtcConnection = webRtcConnection;
            _signallingServerBaseUrl = configuration["SignallingServer:BaseUrl"];
            _logger = logger;
            _jsRuntime = jsRuntime;
            SignallingServerProxy = new SignallingServerProxy.SignallingServerProxy(_signallingServerBaseUrl, this);
            _webRtcConnection.SignallingServerProxy = SignallingServerProxy;
        }

        public async Task<string[]> GetTurnServerNamesAsync()
        {
            var result = await SignallingServerProxy.GetTurnServerNamesAsync();
            if (result.Status != Ardalis.Result.ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
            return result.Value;
        }


//        public IObservable<PeerResponseParameters> ConnectionRequest(
//            ConnectionRequestParameters connectionRequestParameters)
//        {
//            return Observable.Create<PeerResponseParameters>(async observer => 
//            {
//                ConnectionContext connectionContext = null;
//                bool isJoined = false;

//                try
//                {
//                    if (GetConnectionContext(connectionRequestParameters.ConnectionParameters.TurnServerName, 
//                        connectionRequestParameters.ConnectionParameters.RoomName) 
//                            is not null)
//                           observer.OnError(new Exception(
//                               $"Room {connectionRequestParameters.ConnectionParameters.RoomName} is in use"));

//                    connectionContext = new ConnectionContext
//                    {
//                        ConnectionRequestParameters = connectionRequestParameters,
//                        Observer = observer
//                    };
//                    _connectionContexts.Add(connectionContext);

//                    await SignallingServerProxy.JoinRoomAsync(
//                        connectionRequestParameters.ConnectionParameters.TurnServerName,
//                        connectionRequestParameters.ConnectionParameters.RoomName, 
//                        connectionRequestParameters.ConnectionParameters.UserName);
//                    isJoined = true;

////// await OnPeerJoinedAsync("StunOnly", "hello", "iOS");

//                }
//                catch (Exception ex)
//                {
//                    observer.OnError(ex);
//                }

//                return async () =>
//                {
//                    try
//                    {
//                        if (isJoined)
//                            await SignallingServerProxy.LeaveRoomAsync(
//                                connectionRequestParameters.ConnectionParameters.TurnServerName,
//                                connectionRequestParameters.ConnectionParameters.RoomName, 
//                                connectionRequestParameters.ConnectionParameters.UserName);

//                        if (connectionContext is not null)
//                        {
//                            foreach (var peerContext in connectionContext.PeerContexts)
//                            {
//                                peerContext.PeerResponseDisposer.Dispose();
//                                peerContext.PeerConnection.Close();
//                            }
//                            _connectionContexts.Remove(connectionContext);
//                        }
//                    }
//                    catch { };
//                };
//            });
//        }

//        public Task ReplaceOutgoingVideoTracksAsync(string turnServerName, string roomName, 
//            IMediaStreamTrack newVideoTrack)
//        {
//            var connectionContext = GetConnectionContext(turnServerName, roomName);
//            foreach (var peerContext in connectionContext.PeerContexts)
//            {
//                var peerConnection = peerContext.PeerConnection;
//                peerConnection
//                    .GetSenders()
//                    //.Where(sender => sender.Track.Kind == MediaStreamTrackKind.Video)
//                    //.Select(sender => sender.ReplaceTrack(newVideoTrack));
//                    .First(sender => sender.Track.Kind == MediaStreamTrackKind.Video)
//                    .ReplaceTrack(newVideoTrack);
//            }

//            return Task.CompletedTask;
//        }


        private async Task FatalErrorAsync(string message)
        {
            //// TODO: what???
            ///
            await Task.CompletedTask;
        }


        public async ValueTask DisposeAsync()
        {
            await SignallingServerProxy.DisposeAsync();
        }

        //private ConnectionContext GetConnectionContext(string turnServerName, string roomName) =>
        //    _connectionContexts.FirstOrDefault(connectionContext =>
        //        connectionContext.ConnectionRequestParameters.ConnectionParameters.TurnServerName
        //            .Equals(turnServerName, StringComparison.OrdinalIgnoreCase) &&
        //        connectionContext.ConnectionRequestParameters.ConnectionParameters.RoomName.Equals(roomName, 
        //            StringComparison.OrdinalIgnoreCase));

        #region SignallingServerCallbacks

        public async Task OnPeerJoinedAsync(string turnServerName, string roomName, string peerUserName) 
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext =  _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                _logger.LogInformation(
                    $">>>>>>>> OnPeerJoined - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName}");

                await _webRtcConnection.CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: true);
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                var offerDescription = await peerConnection.CreateOffer();
                // Android DOES NOT expose 'Type'!!! I set it manually here. 
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    offerDescription.Type = RTCSdpType.Offer;

                // Send offer before setting local description to avoid race condition with ice candidates.
                // Setting local description triggers ice candidate packets.
                var sdp = JsonSerializer.Serialize(offerDescription, JsonHelper.WebRtcJsonSerializerOptions);
                _logger.LogInformation(
                    $"-------> Sending Offer - room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName}");// sdp:{offerDescription.Sdp}");

                //_logger.LogInformation(
                //    $"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                await peerConnection.SetLocalDescription(offerDescription);

                await SignallingServerProxy.SdpAsync(turnServerName, roomName, peerUserName, sdp);

            }
            catch (Exception ex)
            {
                subject?.OnNext(new PeerResponseParameters 
                { 
                    Code = PeerResponseCode.PeerError,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task OnPeerLeftAsync(string turnServerName, string roomName, string peerUserName)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                //_logger.LogInformation(
                //    $">>>>>>>> OnPeerLeft - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                subject = peerContext.PeerResponseSubject;

                await _webRtcConnection.CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName,
                    isInitiator: peerContext.IsInitiator, isDelete: true);

                subject.OnNext(new PeerResponseParameters 
                {
                    Code = PeerResponseCode.PeerLeft,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                });
            }
            catch (Exception ex)
            {
                subject?.OnNext(new PeerResponseParameters
                {
                    Code = PeerResponseCode.PeerError,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task OnPeerSdpAsync(string turnServerName, string roomName, string peerUserName, string peerSdp)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                _logger.LogInformation(
                    $"<-------- OnPeerSdp - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName}"); //peedSdp:{peerSdp}");

                var description = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    JsonHelper.WebRtcJsonSerializerOptions);

                var peerContext = connectionContext.PeerContexts
                    .FirstOrDefault(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                if (description.Type == RTCSdpType.Offer &&  peerContext is null)
                {
                    await _webRtcConnection.CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: false);
                    peerContext = connectionContext.PeerContexts
                        .Single(context => context.PeerParameters.PeerUserName
                        .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                }
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                //_logger.LogInformation(
                //    $"**** SetRemoteDescription - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                await peerConnection.SetRemoteDescription(description);

                if (description.Type == RTCSdpType.Offer)
                {
                    var answerDescription = await peerConnection.CreateAnswer();
                    // Android DOES NOT expose 'Type'!!! I set it manually here. 
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                        answerDescription.Type = RTCSdpType.Answer;

                    // Send offer before setting local description to avoid race condition with ice candidates.
                    // Setting local description triggers ice candidate packets.
                    var sdp = JsonSerializer.Serialize(answerDescription, JsonHelper.WebRtcJsonSerializerOptions);
                    _logger.LogInformation(
                        $"-------> Sending Answer - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName}  " +
                        $"peerUser:{peerUserName}");// sdp:{answerDescription.Sdp}");

                    //_logger.LogInformation(
                    //    $"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                    //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    //    $"peerUser:{peerUserName}");
                    await peerConnection.SetLocalDescription(answerDescription);
                    await SignallingServerProxy.SdpAsync(turnServerName, roomName, peerUserName, sdp);
                }
            }
            catch (Exception ex)
            {
                subject?.OnNext(new PeerResponseParameters
                {
                    Code = PeerResponseCode.PeerError,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                    ErrorMessage = ex.Message
                });
            }
        }


        public async Task OnPeerIceCandidateAsync(string turnServerName, string roomName, string peerUserName, 
            string peerIce)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                _logger.LogInformation(
                    $"<-------- OnPeerIceCandidate - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName} " +
                    $"peerIce:{peerIce}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(peerIce,
                    JsonHelper.WebRtcJsonSerializerOptions);
                //_logger.LogInformation(
                //    $"**** AddIceCandidate - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                await peerConnection.AddIceCandidate(iceCandidate);
            }
            catch (Exception ex)
            {
                subject?.OnNext(new PeerResponseParameters
                {
                    Code = PeerResponseCode.PeerError,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                    ErrorMessage = ex.Message
                });
            }
        }


        #endregion

        //private async Task CreateOrDeletePeerConnectionAsync(string turnServerName, string roomName, 
        //    string peerUserName, bool isInitiator, bool isDelete = false)
        //{
        //    Subject<PeerResponseParameters> subject = null;
        //    try
        //    {
        //        PeerContext peerContext = null;
        //        IRTCPeerConnection peerConnection = null;
        //        IMediaStream mediaStream = null;
        //        IRTCDataChannel dataChannel = null;

        //        var connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);

        //        if (isDelete)
        //        {
        //            peerContext = connectionContext.PeerContexts
        //                .Single(context => context.PeerParameters.PeerUserName.Equals(peerUserName,
        //                    StringComparison.OrdinalIgnoreCase));
        //            peerConnection = peerContext.PeerConnection;

        //            peerConnection.OnConnectionStateChanged -= OnConnectionStateChanged;
        //            peerConnection.OnDataChannel -= OnDataChannel;
        //            peerConnection.OnIceCandidate -= OnIceCandidate;
        //            peerConnection.OnIceConnectionStateChange -= OnIceConnectionStateChange;
        //            peerConnection.OnIceGatheringStateChange -= OnIceGatheringStateChange;
        //            peerConnection.OnNegotiationNeeded -= OnNegotiationNeeded;
        //            peerConnection.OnSignallingStateChange -= OnSignallingStateChange;
        //            peerConnection.OnTrack -= OnTrack;

        //            // Remove local tracks and close.
        //            var senders = peerConnection.GetSenders();
        //            foreach (var sender in senders)
        //                peerConnection.RemoveTrack(sender);
        //            peerConnection.Close();

        //            connectionContext.PeerContexts.Remove(peerContext);
        //        }
        //        else
        //        {
        //            mediaStream = WebRtcMiddleware.WebRtc.Window(_jsRuntime).MediaStream();

        //            var configuration = new RTCConfiguration
        //            {
        //                IceServers = connectionContext.IceServers ?? await SignallingServerProxy
        //                    .GetIceServersAsync(turnServerName),
        //                //PeerIdentity = peerUserName
        //            };

        //            _logger.LogInformation($"################ LIST OF ICE SERVERS ################");
        //            foreach (var iceServer in configuration.IceServers)
        //                foreach (var url in iceServer.Urls)
        //                    _logger.LogInformation($"\t - {url}");
        //            _logger.LogInformation($"#####################################################");

        //            peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
        //            subject = new Subject<PeerResponseParameters>();
        //            peerContext = new PeerContext
        //            {
        //                PeerParameters = new PeerParameters
        //                {
        //                    TurnServerName = turnServerName,
        //                    RoomName = roomName,
        //                    PeerUserName = peerUserName
        //                },
        //                PeerConnection = peerConnection,
        //                IsInitiator = isInitiator,
        //                PeerResponseSubject = subject,
        //                PeerResponseDisposer = subject
        //                    .AsObservable()
        //                    .Subscribe(connectionContext.Observer.OnNext)
        //            };
        //            connectionContext.PeerContexts.Add(peerContext);

        //            peerConnection.OnConnectionStateChanged += OnConnectionStateChanged;
        //            peerConnection.OnDataChannel += OnDataChannel;
        //            peerConnection.OnIceCandidate += OnIceCandidate;
        //            peerConnection.OnIceConnectionStateChange += OnIceConnectionStateChange;
        //            peerConnection.OnIceGatheringStateChange += OnIceGatheringStateChange;
        //            peerConnection.OnNegotiationNeeded += OnNegotiationNeeded;
        //            peerConnection.OnSignallingStateChange += OnSignallingStateChange;
        //            peerConnection.OnTrack += OnTrack;


        //            if (connectionContext.ConnectionRequestParameters.DataChannelName is not null && isInitiator)
        //            {
        //                dataChannel = peerConnection.CreateDataChannel(
        //                    connectionContext.ConnectionRequestParameters.DataChannelName,
        //                    new RTCDataChannelInit
        //                    {
        //                        Negotiated = false,
        //                    });
        //            }

        //            if (connectionContext.ConnectionRequestParameters.LocalStream is not null)
        //            {
        //                var videoTrack = connectionContext.ConnectionRequestParameters.LocalStream.GetVideoTracks()
        //                    .FirstOrDefault();
        //                var audioTrack = connectionContext.ConnectionRequestParameters.LocalStream.GetAudioTracks()
        //                    .FirstOrDefault();
        //                if (videoTrack is not null)
        //                    peerConnection.AddTrack(videoTrack,
        //                        connectionContext.ConnectionRequestParameters.LocalStream);
        //                if (audioTrack is not null)
        //                    peerConnection.AddTrack(audioTrack,
        //                        connectionContext.ConnectionRequestParameters.LocalStream);
        //            }
        //        }

        //        void OnConnectionStateChanged(object s, EventArgs e)
        //        {
        //            _logger.LogInformation(
        //                $"######## OnConnectionStateChanged - room:{roomName} " +
        //                $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //                $"peerUser:{peerUserName} " +
        //                $"connectionState:{peerConnection.ConnectionState}");
        //            if (peerConnection.ConnectionState == RTCPeerConnectionState.Connected)
        //                peerContext.PeerResponseSubject.OnNext(new PeerResponseParameters
        //                {
        //                    Code = PeerResponseCode.PeerJoined,
        //                    TurnServerName = turnServerName,
        //                    RoomName = roomName,
        //                    PeerUserName = peerUserName,
        //                    MediaStream = mediaStream,
        //                    DataChannel = isInitiator ? dataChannel : null
        //                });
        //            //// WILL BE HANDLED BY PEER LEFT
        //            //else if (peerConnection.ConnectionState == RTCPeerConnectionState.Disconnected)
        //            //ConnectionResponseSubject.OnCompleted();
        //        }
        //        void OnDataChannel(object s, IRTCDataChannelEvent e)
        //        {
        //            _logger.LogInformation(
        //                $"######## OnDataChannel - room:{roomName} " +
        //                $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //                $"peerUser:{peerUserName} " +
        //                $"state:{e.Channel.ReadyState}");

        //            dataChannel?.Close();
        //            dataChannel?.Dispose();

        //            dataChannel = e.Channel;
        //            peerContext.PeerResponseSubject.OnNext(new PeerResponseParameters
        //            {
        //                Code = PeerResponseCode.PeerJoined,
        //                TurnServerName = turnServerName,
        //                RoomName = roomName,
        //                PeerUserName = peerUserName,
        //                MediaStream = null,
        //                DataChannel = dataChannel
        //            });
        //        }
        //        async void OnIceCandidate(object s, IRTCPeerConnectionIceEvent e)
        //        {
        //            //_logger.LogInformation(
        //            //    $"######## OnIceCandidate - room:{roomName} " +
        //            //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //            //    $"peerUser:{peerUserName}");

        //            // 'null' is valid and indicates end of ICE gathering process.
        //            if (e.Candidate is not null)
        //            {
        //                var iceCandidate = new RTCIceCandidateInit
        //                {
        //                    Candidate = e.Candidate.Candidate,
        //                    SdpMid = e.Candidate.SdpMid,
        //                    SdpMLineIndex = e.Candidate.SdpMLineIndex,
        //                    //UsernameFragment = ???
        //                };
        //                var ice = JsonSerializer.Serialize(iceCandidate, JsonHelper.WebRtcJsonSerializerOptions);
        //                _logger.LogInformation(
        //                    $"--------> Sending ICE Candidate - room:{roomName} " +
        //                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //                    $"peerUser:{peerUserName} " +
        //                    $"ice:{ice}");
        //                //if (_isAsyncCall)
        //                    await SignallingServerProxy.IceCandidateAsync(turnServerName, roomName, peerUserName, ice);
        //                //else
        //                    //_signallingServerClient.IceCandidateSync(turnServerName, roomName, peerUserName, ice);
        //            }
        //        }
        //        void OnIceConnectionStateChange(object s, EventArgs e)
        //        {
        //            _logger.LogInformation(
        //                $"######## OnIceConnectionStateChange - room:{roomName} " +
        //                $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //                $"peerUser:{peerUserName} " +
        //                $"iceConnectionState:{peerConnection.IceConnectionState}");
        //        }
        //        void OnIceGatheringStateChange(object s, EventArgs e)
        //        {
        //            _logger.LogInformation(
        //                $"######## OnIceGatheringStateChange - room:{roomName} " +
        //                $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //                $"peerUser:{peerUserName} " +
        //                $"iceGatheringState: {peerConnection.IceGatheringState}");
        //        }
        //        void OnNegotiationNeeded(object s, EventArgs e)
        //        {
        //            _logger.LogInformation(
        //                $"######## OnNegotiationNeeded - room:{roomName} " +
        //                $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //                $"peerUser:{peerUserName}");
        //            // TODO: WHAT IF Not initiator adds track (which trigggers this event)???
        //        }
        //        void OnSignallingStateChange(object s, EventArgs e)
        //        {
        //            _logger.LogInformation(
        //                $"######## OnSignallingStateChange - room:{roomName} " +
        //                $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //                $"peerUser:{peerUserName}, " +
        //                $"signallingState:{ peerConnection.SignalingState }");
        //            //RoomEventSubject.OnNext(new RoomEvent
        //            //{
        //            //    Code = RoomEventCode.PeerJoined,
        //            //    RoomName = roomName,
        //            //    PeerUserName = peerUserName,
        //            //    MediaStream = mediaStream
        //            //});
        //        }
        //        void OnTrack(object s, IRTCTrackEvent e)
        //        {
        //            _logger.LogInformation(
        //                $"######## OnTrack - room:{roomName} " +
        //                $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
        //                $"peerUser:{peerUserName} " +
        //                $"trackType:{e.Track.Kind}");
        //            mediaStream.AddTrack(e.Track);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        subject?.OnNext(new PeerResponseParameters
        //        {
        //            Code = PeerResponseCode.PeerError,
        //            TurnServerName = turnServerName,
        //            RoomName = roomName,
        //            PeerUserName = peerUserName,
        //            ErrorMessage = ex.Message
        //        });
        //    }
        //}

    }
}
