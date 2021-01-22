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
                        //RoomState = RoomState.Idle,
                        JoinRoomRequestParameters = joinRoomRequestParameters
                    };
                    _roomContexts.Add(roomContext);

                    //roomContext.RoomState = RoomState.Connecting;
                    //Console.WriteLine("#### Connecting...");

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
            DebugPrint($"====> OnPeerJoined - turn:{turnServerName} room:{roomName} peerUser:{peerUserName}");
            var roomContext = GetRoomContext(turnServerName, roomName);
            try
            {
//                if (roomContext.RoomState == RoomState.Error)
  //                  return;
    //            if (roomContext.RoomState != RoomState.Connected)
      //              throw new Exception($"Room {roomContext.JoinRoomRequestParameters.RoomName} " +
        //                $"is in wrong state {roomContext.RoomState}");

                var mediaStream = WebRtcMiddleware.WebRtc.Window(_jsRuntime).MediaStream();

                var configuration = new RTCConfiguration
                {
                    IceServers = await _signallingServerClient.GetIceServers(turnServerName),
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
                    await _signallingServerClient.IceCandidate(turnServerName, roomName, peerUserName,
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
                peerConnection.OnNegotiationNeeded += async (s, e) =>
                {
                    DebugPrint($"====> OnPeerJoined.OnNegotiationNeeded - room:{roomName} peerUser:{peerUserName}");
                    //var offerDescription = await peerConnection.CreateOffer();
                    //await peerConnection.SetLocalDescription(offerDescription);
                    //await _signallingServerClient.OfferSdpAsync(roomName, peerUserName,
                    //    JsonSerializer.Serialize(offerDescription, _jsonSerializerOptions));
                };
                peerConnection.OnSignallingStateChange += (s, e) =>
                {
                    DebugPrint($"====> OnPeerSdpOffered.OnSignallingStateChange - room:{roomName} peerUser:{peerUserName}, signallingState:{ peerConnection.SignalingState }");

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
                peerConnection.AddTrack(roomContext.JoinRoomRequestParameters.LocalStream.GetVideoTracks().First(),
                    roomContext.JoinRoomRequestParameters.LocalStream);
                peerConnection.AddTrack(roomContext.JoinRoomRequestParameters.LocalStream.GetAudioTracks().First(),
                    roomContext.JoinRoomRequestParameters.LocalStream);

                var offerDescription = await peerConnection.CreateOffer();
                await peerConnection.SetLocalDescription(offerDescription);
                await _signallingServerClient.OfferSdp(turnServerName, roomName, peerUserName,
                    JsonSerializer.Serialize(offerDescription, _jsonSerializerOptions));
            }
            catch (Exception ex)
            {
                //roomContext.RoomState = RoomState.Error;
                PeerCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerLeft(string turnServerName, string roomName, string peerUserName)
        {
            var roomContext = GetRoomContext(turnServerName, roomName);
            try
            {
//                if (roomContext.RoomState == RoomState.Error)
  //                  return;

            }
            catch (Exception ex)
            {
    //            roomContext.RoomState = RoomState.Error;
                PeerCallbackSubject.OnError(ex);
            }
        }


        public async Task OnPeerSdpOffered(string turnServerName, string roomName, string peerUserName, string peerSdp)
        {
            DebugPrint($"====> OnPeerSdpOffered - room:{roomName} peerUser:{peerUserName}");

            var roomContext = GetRoomContext(turnServerName, roomName);
            try
            {
                //if (roomContext.RoomState == RoomState.Error)
                  //  return;

                //if (roomContext.RoomState != RoomState.Connected)
                  //  throw new Exception($"Room {roomContext.JoinRoomRequestParameters.RoomName} " +
                    //    $"is in wrong state {roomContext.RoomState}");

                var mediaStream = WebRtcMiddleware.WebRtc.Window(_jsRuntime).MediaStream();

                var configuration = new RTCConfiguration
                {
                    IceServers = roomContext.IceServers ?? await _signallingServerClient.GetIceServers(turnServerName),
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
                    await _signallingServerClient.IceCandidate(turnServerName, roomName, peerUserName,
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
                    DebugPrint($"====> OnPeerSdpOffered.OnSignallingStateChange - room:{roomName} peerUser:{peerUserName}, signallingState:{ peerConnection.SignalingState }");
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
                    DebugPrint($"====> OnPeerSdpOffered.OnTrack - room:{roomName} peerUser:{peerUserName}");
                    mediaStream.AddTrack(e.Track);
                    ////roomContext.JoinRoomRequestParameters.LocalStream.AddTrack(e.Track);
                };

                peerConnection.AddTrack(roomContext.JoinRoomRequestParameters.LocalStream.GetVideoTracks().First(),
                    roomContext.JoinRoomRequestParameters.LocalStream);
                peerConnection.AddTrack(roomContext.JoinRoomRequestParameters.LocalStream.GetAudioTracks().First(),
                    roomContext.JoinRoomRequestParameters.LocalStream);

                var offerDescription = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    _jsonSerializerOptions);
                await peerConnection.SetRemoteDescription(offerDescription);

                var answerDescription = await peerConnection.CreateAnswer();
                await peerConnection.SetLocalDescription(answerDescription);
                await _signallingServerClient.AnswerSdp(turnServerName, roomName, peerUserName,
                    JsonSerializer.Serialize(answerDescription, _jsonSerializerOptions));
            }
            catch (Exception ex)
            {
                //roomContext.RoomState = RoomState.Error;
                PeerCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerSdpAnswered(string turnServerName, string roomName, string peerUserName, string peerSdp)
        {
            DebugPrint($"====> OnPeerSdpAnswered - room:{roomName} peerUser:{peerUserName}");

            var roomContext = GetRoomContext(turnServerName, roomName);
            try
            {
                //if (roomContext.RoomState == RoomState.Error)
                  //  return;

                //if (roomContext.RoomState != RoomState.Connected)
                  //  throw new Exception($"Room {roomContext.JoinRoomRequestParameters.RoomName} " +
                    //    $"is in wrong state {roomContext.RoomState}");

                var answerDescription = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    _jsonSerializerOptions);
                var peerConnection = roomContext.PeerConnections.Single(peer => peer.Key == peerUserName)
                    .Value;
                await peerConnection.SetRemoteDescription(answerDescription);
            }
            catch (Exception ex)
            {
                //roomContext.RoomState = RoomState.Error;
                PeerCallbackSubject.OnError(ex);
            }
        }

        public async Task OnPeerIceCandidate(string turnServerName, string roomName, string peerUserName, string peerIce)
        {
            DebugPrint($"====> OnPeerIceCandidate - room:{roomName} peerUser:{peerUserName}");

            var roomContext = GetRoomContext(turnServerName, roomName);
            try
            {
                //if (roomContext.RoomState == RoomState.Error)
                  //  return;

                //if (roomContext.RoomState != RoomState.Connected)
                  //  throw new Exception($"Room {roomContext.JoinRoomRequestParameters.RoomName} " +
                    //    $"is in wrong state {roomContext.RoomState}");

                var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(peerIce,
                    _jsonSerializerOptions);
                var peerConnection = roomContext.PeerConnections.Single(peer => peer.Key == peerUserName)
                    .Value;
                await peerConnection.AddIceCandidate(iceCandidate);
            }
            catch (Exception ex)
            {
                //roomContext.RoomState = RoomState.Error;
                PeerCallbackSubject.OnError(ex);
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
