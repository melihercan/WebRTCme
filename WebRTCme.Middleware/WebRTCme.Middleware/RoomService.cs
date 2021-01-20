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
            self._signallingServerClient = SignallingServerClientFactory.Create(signallingServerBaseUrl);
            await self._signallingServerClient.InitializeAsync(self);
            self._jsRuntime = jsRuntime;
            return self;
        }

        private RoomService() { }


        private Subject<RoomCallbackParameters> RoomCallbackSubject { get; } = new Subject<RoomCallbackParameters>();

        public IObservable<RoomCallbackParameters> RoomRequest(RoomRequestParameters roomRequestParameters)
        {
            return Observable.Create<RoomCallbackParameters>(async observer => 
            {
                IDisposable roomCallbackDisposer = null;
                RoomContext roomContext = null;
                bool isJoined = false;
                bool isStarted = false;

                try
                {
                    roomCallbackDisposer = RoomCallbackSubject
                        .AsObservable()
                        .Subscribe(observer.OnNext);

                    if (RoomContextFromName(roomRequestParameters.RoomName) is not null)
                        observer.OnError(/*throw*/ new Exception($"Room {roomRequestParameters.RoomName} is in use"));

                    roomContext = new RoomContext
                    {
                        RoomState = RoomState.Idle,
                        RoomRequestParameters = roomRequestParameters
                    };
                    _roomContexts.Add(roomContext);

                    roomContext.RoomState = RoomState.Connecting;
                    Console.WriteLine("#### Connecting...");
                    System.Diagnostics.Debug.WriteLine("#### Connecting...");
                    await _signallingServerClient.JoinRoomAsync(roomRequestParameters.RoomName, roomRequestParameters.UserName);
                    isJoined = true;
                    if (roomRequestParameters.IsInitiator)
                    {
                        await _signallingServerClient.StartRoomAsync(roomRequestParameters.RoomName,
                            roomRequestParameters.UserName, roomRequestParameters.TurnServer);
                        isStarted = true;
                    }
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return async () =>
                {
                    roomCallbackDisposer.Dispose();
                    try
                    {
                        if (isJoined)
                            await _signallingServerClient.LeaveRoomAsync(roomRequestParameters.RoomName,
                                roomRequestParameters.UserName);
                        if (isStarted)
                            await _signallingServerClient.StopRoomAsync(roomRequestParameters.RoomName,
                                roomRequestParameters.UserName);
                        if (roomContext is not null)
                            _roomContexts.Remove(roomContext);
                    }
                    catch { };
                };
            });
        }


        public async Task<IMediaStream> ConnectRoomAsync(RoomRequestParameters roomRequestParameters)
        {
            if (RoomContextFromName(roomRequestParameters.RoomName) is not null)
                throw new Exception($"Room {roomRequestParameters.RoomName} is in use");

            var roomContext = new RoomContext
            {
                RoomState = RoomState.Idle,
                RoomRequestParameters = roomRequestParameters
            };
            _roomContexts.Add(roomContext);

            roomContext.RoomState = RoomState.Connecting;
            Console.WriteLine("#### Connecting...");
            System.Diagnostics.Debug.WriteLine("#### Connecting...");
            await _signallingServerClient.JoinRoomAsync(roomRequestParameters.RoomName, roomRequestParameters.UserName);
            if (roomRequestParameters.IsInitiator)
                await _signallingServerClient.StartRoomAsync(roomRequestParameters.RoomName,
                    roomRequestParameters.UserName, roomRequestParameters.TurnServer);



            return await roomContext.ConnectTcs.Task;
        }


        public Task DisconnectRoomAsync(RoomRequestParameters roomRequestParameters)
        {
            throw new NotImplementedException();
        }

        private async Task FatalErrorAsync(string message)
        {
            //// TODO: what???
            ///
            await Task.CompletedTask;
        }


        public ValueTask DisposeAsync()
        {
            return new ValueTask(_signallingServerClient.CleanupAsync());
        }

        private RoomContext RoomContextFromName(string roomName) =>
            _roomContexts.FirstOrDefault(context => context.RoomRequestParameters.RoomName == roomName);


        #region SignallingServerCallbacks
        //// TODO: NOT a good idea to run async ops on callbacks, especially on iOS. Callbacks returning tsks will not be
        /// handled correctly. Create a separate task and schedule callbacks there!!!
        /// Use Task.Channels to create a pipeline. CallbackPipeline.
        public Task OnRoomStarted(string roomName, RTCIceServer[] iceServers)
        {
            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return Task.CompletedTask;
                if (roomContext.RoomState != RoomState.Connecting)
                    throw new Exception($"Room {roomContext.RoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                roomContext.IceServers = iceServers;
                roomContext.RoomState = RoomState.Connected;
                
                RoomCallbackSubject.OnNext(new RoomCallbackParameters 
                { 
                    Code = RoomCallbackCode.RoomStarted,
                    RoomName = roomName
                });
            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                RoomCallbackSubject.OnError(ex);
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
                    throw new Exception($"Room {roomContext.RoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                roomContext.RoomState = RoomState.Disconnected;
                RoomCallbackSubject.OnNext(new RoomCallbackParameters 
                { 
                    Code = RoomCallbackCode.RoomStopped,
                    RoomName = roomName
                });
                RoomCallbackSubject.OnCompleted();
            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                RoomCallbackSubject.OnError(ex);
            }
            return Task.CompletedTask;
        }

        public async Task OnPeerJoined(string roomName, string peerUserName)
        {
            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return;
                if (roomContext.RoomState != RoomState.Connected)
                    throw new Exception($"Room {roomContext.RoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                var mediaStream = WebRtcMiddleware.WebRtc.Window(_jsRuntime).MediaStream();

                var configuration = new RTCConfiguration
                {
                    IceServers = roomContext.IceServers,
                    PeerIdentity = roomName
                };
                var peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
                roomContext.PeerConnections.Add(peerUserName, peerConnection);
                peerConnection.OnConnectionStateChanged += (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnConnectionStateChanged - room:{roomName} peerUser:{peerUserName}");
                };
                peerConnection.OnDataChannel += (s, e) =>
                {
                };
                peerConnection.OnIceCandidate += async (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnIceCandidate - room:{roomName} peerUser:{peerUserName}");
                    var iceCandidate = new RTCIceCandidateInit
                    {
                        Candidate = e.Candidate.Candidate,
                        SdpMid = e.Candidate.SdpMid,
                        SdpMLineIndex = e.Candidate.SdpMLineIndex,
                        //UsernameFragment = ???
                    };
                    await _signallingServerClient.IceCandidateAsync(roomName, peerUserName,
                        JsonSerializer.Serialize(iceCandidate, _jsonSerializerOptions));
                };
                peerConnection.OnIceConnectionStateChange += (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnIceConnectionStateChange - room:{roomName} peerUser:{peerUserName}");
                };
                peerConnection.OnIceGatheringStateChange += (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnIceGatheringStateChange - room:{roomName} peerUser:{peerUserName}");
                };
                peerConnection.OnNegotiationNeeded += (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnNegotiationNeeded - room:{roomName} peerUser:{peerUserName}");
                };
                peerConnection.OnSignallingStateChange += (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnSignallingStateChange - room:{roomName} peerUser:{peerUserName}");

                    //RoomEventSubject.OnNext(new RoomEvent
                    //{
                    //    Code = RoomEventCode.PeerJoined,
                    //    RoomName = roomName,
                    //    PeerUserName = peerUserName,
                    //    MediaStream = mediaStream
                    //});

                };
                peerConnection.OnTrack += (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnTrack - room:{roomName} peerUser:{peerUserName}");
                    mediaStream.AddTrack(e.Track);
                };
                peerConnection.AddTrack(roomContext.RoomRequestParameters.LocalStream.GetVideoTracks().First(),
                    roomContext.RoomRequestParameters.LocalStream);
                peerConnection.AddTrack(roomContext.RoomRequestParameters.LocalStream.GetAudioTracks().First(),
                    roomContext.RoomRequestParameters.LocalStream);

                var offerDescription = await peerConnection.CreateOffer();
                await peerConnection.SetLocalDescription(offerDescription);
                await _signallingServerClient.OfferSdpAsync(roomName, peerUserName,
                    JsonSerializer.Serialize(offerDescription, _jsonSerializerOptions));

            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                RoomCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerLeft(string roomName, string peerUserName)
        {
            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return;

            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                RoomCallbackSubject.OnError(ex);
            }
        }


        public async Task OnPeerSdpOffered(string roomName, string peerUserName, string peerSdp)
        {
            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return;

                if (roomContext.RoomState != RoomState.Connected)
                    throw new Exception($"Room {roomContext.RoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                var mediaStream = WebRtcMiddleware.WebRtc.Window(_jsRuntime).MediaStream();

                var configuration = new RTCConfiguration
                {
                    IceServers = roomContext.IceServers,
                    PeerIdentity = roomName
                };
                var peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
                roomContext.PeerConnections.Add(peerUserName, peerConnection);
                peerConnection.OnConnectionStateChanged += (s, e) =>
                {
                    DebugPrint($"====> OnPeerSdpOffered.OnConnectionStateChanged - room:{roomName} peerUser:{peerUserName}");
                };
                peerConnection.OnDataChannel += (s, e) =>
                {
                };
                peerConnection.OnIceCandidate += async (s, e) =>
                {
                    DebugPrint($"====> OnPeerSdpOffered.OnIceCandidate - room:{roomName} peerUser:{peerUserName}");
                    var iceCandidate = new RTCIceCandidateInit
                    {
                        Candidate = e.Candidate.Candidate,
                        SdpMid = e.Candidate.SdpMid,
                        SdpMLineIndex = e.Candidate.SdpMLineIndex,
                        //UsernameFragment = ???
                    };
                    await _signallingServerClient.IceCandidateAsync(roomName, peerUserName,
                        JsonSerializer.Serialize(iceCandidate, _jsonSerializerOptions));
                };
                peerConnection.OnIceConnectionStateChange += (s, e) =>
                {
                    DebugPrint($"====> OnPeerSdpOffered.OnIceConnectionStateChange - room:{roomName} peerUser:{peerUserName}");
                };
                peerConnection.OnIceGatheringStateChange += (s, e) =>
                {
                    DebugPrint($"====> OnPeerSdpOffered.OnIceGatheringStateChange - room:{roomName} peerUser:{peerUserName}");
                };
                peerConnection.OnNegotiationNeeded += (s, e) =>
                {
                    DebugPrint($"====> OnPeerSdpOffered.OnNegotiationNeeded - room:{roomName} peerUser:{peerUserName}");
                };
                peerConnection.OnSignallingStateChange += (s, e) =>
                {
                    DebugPrint($"====> OnPeerSdpOffered.OnSignallingStateChange - room:{roomName} peerUser:{peerUserName}");
                    //RoomEventSubject.OnNext(new RoomEvent
                    //{
                    //    Code = RoomEventCode.PeerJoined,
                    //    RoomName = roomName,
                    //    PeerUserName = peerUserName,
                    //    MediaStream = mediaStream
                    //});
                };
                peerConnection.OnTrack += (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnTrack - room:{roomName} peerUser:{peerUserName}");
                    mediaStream.AddTrack(e.Track);
                };
                peerConnection.AddTrack(roomContext.RoomRequestParameters.LocalStream.GetVideoTracks().First(),
                    roomContext.RoomRequestParameters.LocalStream);
                peerConnection.AddTrack(roomContext.RoomRequestParameters.LocalStream.GetAudioTracks().First(),
                    roomContext.RoomRequestParameters.LocalStream);

                var offerDescription = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    _jsonSerializerOptions);
                await peerConnection.SetRemoteDescription(offerDescription);

                var answerDescription = await peerConnection.CreateAnswer();
                await peerConnection.SetLocalDescription(answerDescription);
                await _signallingServerClient.AnswerSdpAsync(roomName, peerUserName,
                    JsonSerializer.Serialize(answerDescription, _jsonSerializerOptions));
            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                RoomCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerSdpAnswered(string roomName, string peerUserName, string peerSdp)
        {
            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return;

                if (roomContext.RoomState != RoomState.Connected)
                    throw new Exception($"Room {roomContext.RoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                var answerDescription = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    _jsonSerializerOptions);
                var peerConnection = roomContext.PeerConnections.Single(peer => peer.Key == peerUserName)
                    .Value;
                await peerConnection.SetRemoteDescription(answerDescription);
            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                RoomCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerIceCandidate(string roomName, string peerUserName, string peerIce)
        {
            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return;

                if (roomContext.RoomState != RoomState.Connected)
                    throw new Exception($"Room {roomContext.RoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(peerIce,
                    _jsonSerializerOptions);
                var peerConnection = roomContext.PeerConnections.Single(peer => peer.Key == peerUserName)
                    .Value;
                await peerConnection.AddIceCandidate(iceCandidate);
            }
            catch (Exception ex)
            {
                roomContext.RoomState = RoomState.Error;
                RoomCallbackSubject.OnError(ex);
            }
        }


        #endregion

        private static void DebugPrint(string message)
        {
            Console.WriteLine(message);
            //System.Diagnostics.Debug.WriteLine(message);
        }


    }
}
