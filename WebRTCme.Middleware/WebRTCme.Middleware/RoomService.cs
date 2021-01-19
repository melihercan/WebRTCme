using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private async Task AbortConnectionAsync(RoomContext roomContext, string message)
        {
            roomContext.RoomState = RoomState.Error;
            await _signallingServerClient.LeaveRoomAsync(roomContext.RoomRequestParameters.RoomName,
                roomContext.RoomRequestParameters.UserName);
            if (roomContext.RoomRequestParameters.IsInitiator)
                await _signallingServerClient.StopRoomAsync(roomContext.RoomRequestParameters.RoomName,
                    roomContext.RoomRequestParameters.UserName);
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
        public async Task OnRoomStarted(string roomName, RTCIceServer[] iceServers)
        {
            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return;
                if (roomContext.RoomState != RoomState.Connecting)
                    throw new Exception($"Room {roomContext.RoomRequestParameters.RoomName} " +
                        $"is in wrong state {roomContext.RoomState}");

                roomContext.IceServers = iceServers;
                roomContext.RoomState = RoomState.Connected;
            }
            catch (Exception ex)
            {
                await AbortConnectionAsync(roomContext, ex.Message);
            }
        }

        public async Task OnRoomStopped(string roomName)
        {
            var roomContext = RoomContextFromName(roomName);
            try
            {
                if (roomContext.RoomState == RoomState.Error)
                    return;

            }
            catch (Exception ex)
            {

            }
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

                var configuration = new RTCConfiguration
                {
                    IceServers = roomContext.IceServers,
                    PeerIdentity = roomName
                };
                var peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
                roomContext.PeerConnections.Add(peerUserName, peerConnection);
                peerConnection.OnConnectionStateChanged += (s, e) =>
                {
                };
                peerConnection.OnDataChannel += (s, e) =>
                {
                };
                peerConnection.OnIceCandidate += async (s, e) =>
                {
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
                };
                peerConnection.OnIceGatheringStateChange += (s, e) =>
                {
                };
                peerConnection.OnNegotiationNeeded += (s, e) =>
                {
                };
                peerConnection.OnSignallingStateChange += (s, e) =>
                {
                };
                peerConnection.OnTrack += (s, e) =>
                {
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
                await AbortConnectionAsync(roomContext, ex.Message);
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
                await AbortConnectionAsync(roomContext, ex.Message);
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

                var configuration = new RTCConfiguration
                {
                    IceServers = roomContext.IceServers,
                    PeerIdentity = roomName
                };
                var peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
                roomContext.PeerConnections.Add(peerUserName, peerConnection);
                peerConnection.OnConnectionStateChanged += (s, e) =>
                {
                };
                peerConnection.OnDataChannel += (s, e) =>
                {
                };
                peerConnection.OnIceCandidate += async (s, e) =>
                {
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
                };
                peerConnection.OnIceGatheringStateChange += (s, e) =>
                {
                };
                peerConnection.OnNegotiationNeeded += (s, e) =>
                {
                };
                peerConnection.OnSignallingStateChange += (s, e) =>
                {
                };
                peerConnection.OnTrack += (s, e) =>
                {
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
                await AbortConnectionAsync(roomContext, ex.Message);
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
                await AbortConnectionAsync(roomContext, ex.Message);
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
                await AbortConnectionAsync(roomContext, ex.Message);
            }
        }


        #endregion

        private static void DebugPrint(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }


    }
}
