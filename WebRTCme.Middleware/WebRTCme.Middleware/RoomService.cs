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

namespace WebRtcMeMiddleware
{
    internal class RoomService : IRoomService, ISignallingServerCallbacks
    {
        private ISignallingServerClient _signallingServerClient;
        private IJSRuntime _jsRuntime;
        private static List<RoomContext> _roomContexts = new();

        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        static public async Task<IRoomService> CreateAsync(string signallingServerBaseUrl, IJSRuntime jsRuntime = null)
        {
            var self = new RoomService();
            self._signallingServerClient = await SignallingServerClientFactory.CreateAsync(signallingServerBaseUrl, 
                self);
            self._jsRuntime = jsRuntime;
            return self;
        }

        private RoomService() { }


        public async Task<string[]> GetTurnServerNames()
        {
            var result = await _signallingServerClient.GetTurnServerNames();
            if (result.Status != Ardalis.Result.ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
            return result.Value;
        }

        private Subject<PeerCallbackParameters> PeerCallbackSubject { get; } = new Subject<PeerCallbackParameters>();

        public IObservable<PeerCallbackParameters> JoinRoomRequest(JoinRoomRequestParameters joinRoomRequestParameters)
        {
            return Observable.Create<PeerCallbackParameters>(async observer => 
            {
                IDisposable peerCallbackDisposer = null;
                RoomContext roomContext = null;
                bool isJoined = false;

                try
                {
                    peerCallbackDisposer = PeerCallbackSubject
                        .AsObservable()
                        .Subscribe(observer.OnNext);

                    if (GetRoomContext(joinRoomRequestParameters.TurnServerName, joinRoomRequestParameters.RoomName) 
                            is not null)
                           observer.OnError(new Exception($"Room {joinRoomRequestParameters.RoomName} is in use"));

                    roomContext = new RoomContext
                    {
                        JoinRoomRequestParameters = joinRoomRequestParameters
                    };
                    _roomContexts.Add(roomContext);

                    await _signallingServerClient.JoinRoom(joinRoomRequestParameters.TurnServerName,
                        joinRoomRequestParameters.RoomName, joinRoomRequestParameters.UserName);
                    isJoined = true;
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return async () =>
                {
                    peerCallbackDisposer.Dispose();
                    try
                    {
                        if (isJoined)
                            await _signallingServerClient.LeaveRoom(joinRoomRequestParameters.TurnServerName,
                                joinRoomRequestParameters.RoomName, joinRoomRequestParameters.UserName);
                        if (roomContext is not null)
                            _roomContexts.Remove(roomContext);
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

        private RoomContext GetRoomContext(string turnServerName, string roomName) =>
            _roomContexts.FirstOrDefault(context =>
                context.JoinRoomRequestParameters.TurnServerName
                    .Equals(turnServerName, StringComparison.OrdinalIgnoreCase) &&
                context.JoinRoomRequestParameters.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));

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
            try
            {
                var roomContext = GetRoomContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerJoined - turn:{turnServerName} room:{roomName} " +
                    $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}");

                await CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: true);
                var peerConnection = roomContext.PeerConnectionContexts
                    .Single(context => context.PeerUserName.Equals(peerUserName, StringComparison.OrdinalIgnoreCase))
                    .PeerConnection;
            }
            catch (Exception ex)
            {
                PeerCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerLeft(string turnServerName, string roomName, string peerUserName)
        {
            var roomContext = GetRoomContext(turnServerName, roomName);
            try
            {

            }
            catch (Exception ex)
            {
                PeerCallbackSubject.OnError(ex);
            }
        }


        public async Task OnPeerSdpOffered(string turnServerName, string roomName, string peerUserName, string peerSdp)
        {

            try
            {
                var roomContext = GetRoomContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerSdpOffered - turn:{turnServerName} room:{roomName} " +
                    $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}"); //peedSdp:{peerSdp}");
                var peerConnection = roomContext.PeerConnectionContexts.Count == 0 ? null : 
                    roomContext.PeerConnectionContexts.FirstOrDefault(context => context.PeerUserName
                        .Equals(peerUserName, StringComparison.OrdinalIgnoreCase)).PeerConnection;
                if (peerConnection is null)
                {
                    await CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: false);
                    peerConnection = roomContext.PeerConnectionContexts
                        .Single(context => context.PeerUserName
                            .Equals(peerUserName, StringComparison.OrdinalIgnoreCase)).PeerConnection;
                }

                var offerDescription = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    _jsonSerializerOptions);
                await peerConnection.SetRemoteDescription(offerDescription);


                var answerDescription = await peerConnection.CreateAnswer();
                await peerConnection.SetLocalDescription(answerDescription);
                var sdp = JsonSerializer.Serialize(answerDescription, _jsonSerializerOptions);
                await _signallingServerClient.AnswerSdp(turnServerName, roomName, peerUserName, sdp);
                DebugPrint($"######## Sending Answer - room:{roomName} " +
                    $"user:{roomContext.JoinRoomRequestParameters.UserName}  peerUser:{peerUserName}");// sdp:{sdp}");
            }
            catch (Exception ex)
            {
                PeerCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerSdpAnswered(string turnServerName, string roomName, string peerUserName, 
            string peerSdp)
        {

            try
            {
                var roomContext = GetRoomContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerSdpAnswered - turn:{turnServerName} room:{roomName} " +
                    $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}");// peerSdp:{peerSdp}");

                var answerDescription = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    _jsonSerializerOptions);
                var peerConnection = roomContext.PeerConnectionContexts
                    .Single(peer => peer.PeerUserName.Equals(peerUserName, StringComparison.OrdinalIgnoreCase))
                    .PeerConnection;
                await peerConnection.SetRemoteDescription(answerDescription);
            }
            catch (Exception ex)
            {
                PeerCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerIceCandidate(string turnServerName, string roomName, string peerUserName, 
            string peerIce)
        {
            try
            {
                var roomContext = GetRoomContext(turnServerName, roomName);
                DebugPrint($">>>>>>>> OnPeerIceCandidate - turn:{turnServerName} room:{roomName} " +
                    $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName} peerIce:{peerIce}");

                var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(peerIce,
                    _jsonSerializerOptions);
                var peerConnection = roomContext.PeerConnectionContexts
                    .Single(context => context.PeerUserName.Equals(peerUserName, StringComparison.OrdinalIgnoreCase))
                    .PeerConnection;
                await peerConnection.AddIceCandidate(iceCandidate);
            }
            catch (Exception ex)
            {
                PeerCallbackSubject.OnError(ex);
            }
        }


#endregion

        private async Task CreateOrDeletePeerConnectionAsync(string turnServerName, string roomName, 
            string peerUserName, bool isInitiator, bool isDelete = false)
        {
            try
            {
                PeerConnectionContext peerConnectionContext = null;
                IRTCPeerConnection peerConnection = null;
                IMediaStream mediaStream = null;

                var roomContext = GetRoomContext(turnServerName, roomName);
                
                if (isDelete)
                {
                    peerConnectionContext = roomContext.PeerConnectionContexts
                        .Single(peer => peer.PeerUserName.Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                    peerConnection = peerConnectionContext.PeerConnection;

                    peerConnection.OnConnectionStateChanged -= OnConnectionStateChanged;
                    peerConnection.OnDataChannel -= OnDataChannel;
                    peerConnection.OnIceCandidate -= OnIceCandidate;
                    peerConnection.OnIceConnectionStateChange -= OnIceConnectionStateChange;
                    peerConnection.OnIceGatheringStateChange -= OnIceGatheringStateChange;
                    peerConnection.OnNegotiationNeeded -= OnNegotiationNeeded;
                    peerConnection.OnSignallingStateChange -= OnSignallingStateChange;
                    peerConnection.OnTrack -= OnTrack;

                    roomContext.PeerConnectionContexts.Remove(peerConnectionContext);
                }
                else
                {
                    mediaStream = WebRtcMiddleware.WebRtc.Window(_jsRuntime).MediaStream();

                    var configuration = new RTCConfiguration
                    {
                        IceServers = roomContext.IceServers ?? await _signallingServerClient
                            .GetIceServers(turnServerName),
                        PeerIdentity = roomName
                    };
                    peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
                    peerConnectionContext = new PeerConnectionContext
                    {
                        PeerUserName = peerUserName,
                        PeerConnection = peerConnection,
                        IsInitiator = isInitiator
                    };
                    roomContext.PeerConnectionContexts.Add(peerConnectionContext);

                    peerConnection.OnConnectionStateChanged += OnConnectionStateChanged;
                    peerConnection.OnDataChannel += OnDataChannel;
                    peerConnection.OnIceCandidate += OnIceCandidate;
                    peerConnection.OnIceConnectionStateChange += OnIceConnectionStateChange;
                    peerConnection.OnIceGatheringStateChange += OnIceGatheringStateChange;
                    peerConnection.OnNegotiationNeeded += OnNegotiationNeeded;
                    peerConnection.OnSignallingStateChange += OnSignallingStateChange;
                    peerConnection.OnTrack += OnTrack;

                    peerConnection.AddTrack(roomContext.JoinRoomRequestParameters.LocalStream.GetVideoTracks().First(),
                        roomContext.JoinRoomRequestParameters.LocalStream);
                    peerConnection.AddTrack(roomContext.JoinRoomRequestParameters.LocalStream.GetAudioTracks().First(),
                        roomContext.JoinRoomRequestParameters.LocalStream);
                }

                void OnConnectionStateChanged(object s, EventArgs e)
                {
                    DebugPrint($"====> OnConnectionStateChanged - room:{roomName} " +
                        $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName} " +
                        $"connectionState:{peerConnection.ConnectionState}");
                    if (peerConnection.ConnectionState == RTCPeerConnectionState.Connected)
                        PeerCallbackSubject.OnNext(new PeerCallbackParameters
                        {
                            Code = PeerCallbackCode.PeerJoined,
                            TurnServerName = turnServerName,
                            RoomName = roomName,
                            PeerUserName = peerUserName,
                            MediaStream = mediaStream
                        });
                    else if (peerConnection.ConnectionState == RTCPeerConnectionState.Disconnected)
                        PeerCallbackSubject.OnCompleted();
                }
                void OnDataChannel(object s, IRTCDataChannelEvent e)
                {
                }
                async void OnIceCandidate(object s, IRTCPeerConnectionIceEvent e)
                {
                    DebugPrint($"====> OnIceCandidate - room:{roomName} " +
                        $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}");

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
                            $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName} ice:{ice}");
                        await _signallingServerClient.IceCandidate(turnServerName, roomName, peerUserName, ice);

                    }
                }
                void OnIceConnectionStateChange(object s, EventArgs e)
                {
                    DebugPrint($"====> OnIceConnectionStateChange - room:{roomName} " +
                        $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName} " +
                        $"iceConnectionState:{peerConnection.IceConnectionState}");
                }
                void OnIceGatheringStateChange(object s, EventArgs e)
                {
                    DebugPrint($"====> OnIceGatheringStateChange - room:{roomName} " +
                        $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName} " +
                        $"iceGatheringState: {peerConnection.IceGatheringState}");
                }
                async void OnNegotiationNeeded(object s, EventArgs e)
                {
                    DebugPrint($"====> OnNegotiationNeeded - room:{roomName} " +
                        $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}");
                    //// TODO: WHAT IF Not initiator adds track (which trigggers this event)???
                    
                    if (peerConnectionContext.IsInitiator)
                    {
                        var offerDescription = await peerConnection.CreateOffer();
                        await peerConnection.SetLocalDescription(offerDescription);
                        var sdp = JsonSerializer.Serialize(offerDescription, _jsonSerializerOptions);
                        DebugPrint($"######## Sending Offer - room:{roomName} " +
                            $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}");// sdp:{sdp}");
                        await _signallingServerClient.OfferSdp(turnServerName, roomName, peerUserName, sdp);
                    }
                }
                void OnSignallingStateChange(object s, EventArgs e)
                {
                    DebugPrint($"====> OnSignallingStateChange - room:{roomName} " +
                        $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}, " +
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
                        $"user:{roomContext.JoinRoomRequestParameters.UserName} peerUser:{peerUserName}");
                    mediaStream.AddTrack(e.Track);
                }
            }
            catch (Exception ex)
            {
                //roomContext.RoomState = RoomState.Error;
                PeerCallbackSubject.OnError(ex);
            }

        }

        private static void DebugPrint(string message)
        {
            Console.WriteLine(message);
            //System.Diagnostics.Debug.WriteLine(message);
        }


    }
}
