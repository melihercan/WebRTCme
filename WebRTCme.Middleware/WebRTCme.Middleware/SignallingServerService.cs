using Microsoft.Extensions.Configuration;
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
using WebRTCme.SignallingServerClient;
using WebRtcMeMiddleware.Models;
using Xamarin.Essentials;

namespace WebRtcMeMiddleware
{
    internal class SignallingServerService : ISignallingServerService, ISignallingServerCallbacks
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly string _signallingServerBaseUrl;
        private ISignallingServerClient _signallingServerClient;
        private static List<ConnectionContext> _connectionContexts = new();

        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        static public async Task<ISignallingServerService> CreateAsync(/*string signallingServerBaseUrl*/
            IConfiguration configuration, 
            IJSRuntime jsRuntime = null)
        {
            var self = new SignallingServerService(/*signallingServerBaseUrl*/configuration, jsRuntime);
            await self.Initialization;
            return self;
        }

        public SignallingServerService(/*string signallingServerBaseUrl*/IConfiguration configuration, 
            IJSRuntime jsRuntime = null)
        {
            _signallingServerBaseUrl = configuration["SignallingServer:BaseUrl"];// signallingServerBaseUrl;
            _jsRuntime = jsRuntime;
            Initialization = InitializeAsync();
        }

        public Task Initialization { get; private set; }

        private async Task InitializeAsync()
        {
            _signallingServerClient = await SignallingServerClientFactory.CreateAsync(_signallingServerBaseUrl,
                this);
        }

        public async Task<string[]> GetTurnServerNames()
        {
            var result = await _signallingServerClient.GetTurnServerNames();
            if (result.Status != Ardalis.Result.ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
            return result.Value;
        }

        public IObservable<PeerResponseParameters> ConnectionRequest(
            ConnectionRequestParameters connectionRequestParameters)
        {
            return Observable.Create<PeerResponseParameters>(async observer => 
            {
                ConnectionContext connectionContext = null;
                bool isJoined = false;

                try
                {
                    if (GetConnectionContext(connectionRequestParameters.TurnServerName, 
                        connectionRequestParameters.RoomName) 
                            is not null)
                           observer.OnError(new Exception($"Room {connectionRequestParameters.RoomName} is in use"));

                    connectionContext = new ConnectionContext
                    {
                        ConnectionRequestParameters = connectionRequestParameters,
                        Observer = observer
                    };
                    _connectionContexts.Add(connectionContext);

                    await _signallingServerClient.JoinRoom(connectionRequestParameters.TurnServerName,
                        connectionRequestParameters.RoomName, connectionRequestParameters.UserName);
                    isJoined = true;
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return async () =>
                {
                    try
                    {
                        if (isJoined)
                            await _signallingServerClient.LeaveRoom(connectionRequestParameters.TurnServerName,
                                connectionRequestParameters.RoomName, connectionRequestParameters.UserName);

                        if (connectionContext is not null)
                        {
                            foreach (var peerContext in connectionContext.PeerContexts)
                                peerContext.PeerResponseDisposer.Dispose();
                            _connectionContexts.Remove(connectionContext);
                        }
                    }
                    catch { };
                };
            });
        }

        private async Task FatalErrorAsync(string message)
        {
            //// TODO: what???
            ///
            await Task.CompletedTask;
        }


        public async ValueTask DisposeAsync()
        {
            await _signallingServerClient.DisposeAsync();
        }

        private ConnectionContext GetConnectionContext(string turnServerName, string roomName) =>
            _connectionContexts.FirstOrDefault(connectionContext =>
                connectionContext.ConnectionRequestParameters.TurnServerName
                    .Equals(turnServerName, StringComparison.OrdinalIgnoreCase) &&
                connectionContext.ConnectionRequestParameters.RoomName.Equals(roomName, 
                    StringComparison.OrdinalIgnoreCase));

        #region SignallingServerCallbacks
        //// TODO: NOT a good idea to run async ops on callbacks, especially on iOS. Callbacks returning tsks will not be
        /// handled correctly. Create a separate task and schedule callbacks there!!!
        /// Use Task.Channels to create a pipeline. CallbackPipeline.
#if false
        public Task OnRoomStarted(string roomName, RTCIceServer[] iceServers)
        {
            DebugPrint($"====> OnRoomStarted - room:{roomName}");

            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return Task.CompletedTask;
                if (roomContext.RoomState != RoomState.Connecting)
                    throw new Exception($"Room {roomContext.JoinRoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                roomContext.IceServers = iceServers;
                roomContext.RoomState = RoomState.Connected;
                
                PeerCallbackSubject.OnNext(new PeerCallbackParameters 
                { 
                    Code = PeerCallbackCode.RoomStarted,
                    RoomName = roomName
                });
            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                PeerCallbackSubject.OnError(ex);
            }
            return Task.CompletedTask;
        }

        public Task OnRoomStopped(string roomName)
        {
            var roomContext = RoomContextFromName(roomName);
            if (roomContext is null)
                return Task.CompletedTask;
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return Task.CompletedTask;

                if (roomContext.RoomState != RoomState.Connected)
                    throw new Exception($"Room {roomContext.JoinRoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                roomContext.RoomState = RoomState.Disconnected;
                PeerCallbackSubject.OnNext(new PeerCallbackParameters 
                { 
                    Code = PeerCallbackCode.RoomStopped,
                    RoomName = roomName
                });
                PeerCallbackSubject.OnCompleted();
            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                PeerCallbackSubject.OnError(ex);
            }
            return Task.CompletedTask;
        }
#endif
        public async Task OnPeerJoined(string turnServerName, string roomName, string peerUserName) 
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = GetConnectionContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerJoined - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");

                await CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: true);
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;


                var offerDescription = await peerConnection.CreateOffer();
                // Android DOES NOT expose 'Type'!!! I set it manually here. 
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    offerDescription.Type = RTCSdpType.Offer;
                
               
                var sdp = JsonSerializer.Serialize(offerDescription, _jsonSerializerOptions);
                DebugPrint($"######## Sending Offer - room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");// sdp:{sdp}");
                await _signallingServerClient.OfferSdp(turnServerName, roomName, peerUserName, sdp);

                DebugPrint($"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
                await peerConnection.SetLocalDescription(offerDescription);
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

        public async Task OnPeerLeft(string turnServerName, string roomName, string peerUserName)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = GetConnectionContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerLeft - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                subject = peerContext.PeerResponseSubject;

                await CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName,
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


        public async Task OnPeerSdpOffered(string turnServerName, string roomName, string peerUserName, string peerSdp)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = GetConnectionContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerSdpOffered - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}"); //peedSdp:{peerSdp}");
                var peerContext = connectionContext.PeerContexts
                    .FirstOrDefault(context => context.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                if (peerContext is null)
                {
                    await CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: false);
                    peerContext = connectionContext.PeerContexts
                        .Single(context => context.PeerUserName
                        .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                }
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                var offerDescription = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    _jsonSerializerOptions);
                DebugPrint($"**** SetRemoteDescription - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
                await peerConnection.SetRemoteDescription(offerDescription);

                var answerDescription = await peerConnection.CreateAnswer();
                // Android DOES NOT expose 'Type'!!! I set it manually here. 
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    offerDescription.Type = RTCSdpType.Answer;

                var sdp = JsonSerializer.Serialize(answerDescription, _jsonSerializerOptions);
                await _signallingServerClient.AnswerSdp(turnServerName, roomName, peerUserName, sdp);
                DebugPrint($"######## Sending Answer - room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName}  peerUser:{peerUserName}");// sdp:{sdp}");

                DebugPrint($"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
                await peerConnection.SetLocalDescription(answerDescription);
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

        public async Task OnPeerSdpAnswered(string turnServerName, string roomName, string peerUserName, 
            string peerSdp)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = GetConnectionContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerSdpAnswered - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");// peerSdp:{peerSdp}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                var answerDescription = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    _jsonSerializerOptions);
                DebugPrint($"**** SetRemoteDescription - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
                await peerConnection.SetRemoteDescription(answerDescription);
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

        public async Task OnPeerIceCandidate(string turnServerName, string roomName, string peerUserName, 
            string peerIce)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = GetConnectionContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerIceCandidate - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName} " +
                    $"peerIce:{peerIce}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(peerIce,
                    _jsonSerializerOptions);
                DebugPrint($"**** AddIceCandidate - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
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

        private async Task CreateOrDeletePeerConnectionAsync(string turnServerName, string roomName, 
            string peerUserName, bool isInitiator, bool isDelete = false)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                PeerContext peerContext = null;
                IRTCPeerConnection peerConnection = null;
                IMediaStream mediaStream = null;
                IRTCDataChannel dataChannel = null;

                var connectionContext = GetConnectionContext(turnServerName, roomName);
                
                if (isDelete)
                {
                    peerContext = connectionContext.PeerContexts
                        .Single(peer => peer.PeerUserName.Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                    peerConnection = peerContext.PeerConnection;

                    peerConnection.OnConnectionStateChanged -= OnConnectionStateChanged;
                    peerConnection.OnDataChannel -= OnDataChannel;
                    peerConnection.OnIceCandidate -= OnIceCandidate;
                    peerConnection.OnIceConnectionStateChange -= OnIceConnectionStateChange;
                    peerConnection.OnIceGatheringStateChange -= OnIceGatheringStateChange;
                    peerConnection.OnNegotiationNeeded -= OnNegotiationNeeded;
                    peerConnection.OnSignallingStateChange -= OnSignallingStateChange;
                    peerConnection.OnTrack -= OnTrack;

                    // Remove local tracks and close.
                    var senders = peerConnection.GetSenders();
                    foreach (var sender in senders)
                        peerConnection.RemoveTrack(sender);
                    peerConnection.Close();

                    connectionContext.PeerContexts.Remove(peerContext);
                }
                else
                {
                    mediaStream = WebRtcMiddleware.WebRtc.Window(_jsRuntime).MediaStream();

                    var configuration = new RTCConfiguration
                    {
                        IceServers = connectionContext.IceServers ?? await _signallingServerClient
                            .GetIceServers(turnServerName),
                        PeerIdentity = roomName
                    };
                    peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
                    subject = new Subject<PeerResponseParameters>();
                    peerContext = new PeerContext
                    {
                        PeerUserName = peerUserName,
                        PeerConnection = peerConnection,
                        IsInitiator = isInitiator,
                        PeerResponseSubject = subject,
                        PeerResponseDisposer = subject
                            .AsObservable()
                            .Subscribe(connectionContext.Observer.OnNext)
                    };
                    connectionContext.PeerContexts.Add(peerContext);

                    peerConnection.OnConnectionStateChanged += OnConnectionStateChanged;
                    peerConnection.OnDataChannel += OnDataChannel;
                    peerConnection.OnIceCandidate += OnIceCandidate;
                    peerConnection.OnIceConnectionStateChange += OnIceConnectionStateChange;
                    peerConnection.OnIceGatheringStateChange += OnIceGatheringStateChange;
                    peerConnection.OnNegotiationNeeded += OnNegotiationNeeded;
                    peerConnection.OnSignallingStateChange += OnSignallingStateChange;
                    peerConnection.OnTrack += OnTrack;

                    
                    if (connectionContext.ConnectionRequestParameters.DataChannelName is not null && isInitiator)
                    {
                        dataChannel = peerConnection.CreateDataChannel(
                            connectionContext.ConnectionRequestParameters.DataChannelName,
                            new RTCDataChannelInit
                            { 
                                Negotiated = false,
                            });
                    }

                    if (connectionContext.ConnectionRequestParameters.LocalStream is not null)
                    {
                        var videoTrack = connectionContext.ConnectionRequestParameters.LocalStream.GetVideoTracks()
                            .FirstOrDefault();
                        var audioTrack = connectionContext.ConnectionRequestParameters.LocalStream.GetAudioTracks()
                            .FirstOrDefault();
                        if (videoTrack is not null)
                            peerConnection.AddTrack(videoTrack, 
                                connectionContext.ConnectionRequestParameters.LocalStream);
                        if (audioTrack is not null)
                            peerConnection.AddTrack(audioTrack,
                                connectionContext.ConnectionRequestParameters.LocalStream);
                    }
                }

                void OnConnectionStateChanged(object s, EventArgs e)
                {
                    DebugPrint($"====> OnConnectionStateChanged - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName} " +
                        $"connectionState:{peerConnection.ConnectionState}");
                    if (peerConnection.ConnectionState == RTCPeerConnectionState.Connected)
                        peerContext.PeerResponseSubject.OnNext(new PeerResponseParameters
                        {
                            Code = PeerResponseCode.PeerJoined,
                            TurnServerName = turnServerName,
                            RoomName = roomName,
                            PeerUserName = peerUserName,
                            MediaStream = mediaStream,
                            DataChannel = isInitiator ? dataChannel : null
                        });
                    //// WILL BE HANDLED BY PEER LEFT
                    //else if (peerConnection.ConnectionState == RTCPeerConnectionState.Disconnected)
                        //ConnectionResponseSubject.OnCompleted();
                }
                void OnDataChannel(object s, IRTCDataChannelEvent e)
                {
                    DebugPrint($"====> OnDataChannel - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
                    dataChannel = e.Channel;
                    peerContext.PeerResponseSubject.OnNext(new PeerResponseParameters
                    {
                        Code = PeerResponseCode.PeerJoined,
                        TurnServerName = turnServerName,
                        RoomName = roomName,
                        PeerUserName = peerUserName,
                        MediaStream = null,
                        DataChannel = dataChannel
                    });
                }
                async void OnIceCandidate(object s, IRTCPeerConnectionIceEvent e)
                {
                    DebugPrint($"====> OnIceCandidate - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");

                    // 'null' is valid and indicates end of ICE gathering process.
                    if (e.Candidate is not null)
                    {
                        var iceCandidate = new RTCIceCandidateInit
                        {
                            Candidate = e.Candidate.Candidate,
                            SdpMid = e.Candidate.SdpMid,
                            SdpMLineIndex = e.Candidate.SdpMLineIndex,
                            //UsernameFragment = ???
                        };
                        var ice = JsonSerializer.Serialize(iceCandidate, _jsonSerializerOptions);
                        DebugPrint($"######## Sending ICE Candidate - room:{roomName} " +
                            $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName} " +
                            $"ice:{ice}");
                        await _signallingServerClient.IceCandidate(turnServerName, roomName, peerUserName, ice);

                    }
                }
                void OnIceConnectionStateChange(object s, EventArgs e)
                {
                    DebugPrint($"====> OnIceConnectionStateChange - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName} " +
                        $"iceConnectionState:{peerConnection.IceConnectionState}");
                }
                void OnIceGatheringStateChange(object s, EventArgs e)
                {
                    DebugPrint($"====> OnIceGatheringStateChange - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName} " +
                        $"iceGatheringState: {peerConnection.IceGatheringState}");
                }
                void OnNegotiationNeeded(object s, EventArgs e)
                {
                    DebugPrint($"====> OnNegotiationNeeded - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
                    //// TODO: WHAT IF Not initiator adds track (which trigggers this event)???

#if false
                    if (peerConnectionContext.IsInitiator)
                    {
                        var offerDescription = await peerConnection.CreateOffer();
                        DebugPrint($"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                            $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}");
                        // Android DOES NOT expose 'Type'!!! I set it manually here. 
                        if (DeviceInfo.Platform == DevicePlatform.Android)
                            offerDescription.Type = RTCSdpType.Offer;
                        await peerConnection.SetLocalDescription(offerDescription);
                        var sdp = JsonSerializer.Serialize(offerDescription, _jsonSerializerOptions);
                        DebugPrint($"######## Sending Offer - room:{roomName} " +
                            $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}");// sdp:{sdp}");
                        await _signallingServerClient.OfferSdp(turnServerName, roomName, peerUserName, sdp);
                    }
#endif
                }
                void OnSignallingStateChange(object s, EventArgs e)
                {
                    DebugPrint($"====> OnSignallingStateChange - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}, " +
                        $"signallingState:{ peerConnection.SignalingState }");
                    //RoomEventSubject.OnNext(new RoomEvent
                    //{
                    //    Code = RoomEventCode.PeerJoined,
                    //    RoomName = roomName,
                    //    PeerUserName = peerUserName,
                    //    MediaStream = mediaStream
                    //});
                }
                void OnTrack(object s, IRTCTrackEvent e)
                {
                    DebugPrint($"====> OnTrack - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.UserName} peerUser:{peerUserName}");
                    mediaStream.AddTrack(e.Track);
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

        private static void DebugPrint(string message)
        {
            Console.WriteLine(message);
            //System.Diagnostics.Debug.WriteLine(message);
        }


    }
}
