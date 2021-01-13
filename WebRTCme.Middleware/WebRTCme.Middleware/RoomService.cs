using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private List<RoomContext> _roomContexts = new();

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
            var roomContext = new RoomContext
            {
                RoomState = RoomState.Idle,
                RoomRequestParameters = roomRequestParameters
            };
            _roomContexts.Add(roomContext);

            await _signallingServerClient.JoinRoomAsync(roomRequestParameters.RoomId, roomRequestParameters.UserId);

            if (roomRequestParameters.IsInitiator)
                await _signallingServerClient.StartRoomAsync(roomRequestParameters.RoomId, roomRequestParameters.UserId, 
                    roomRequestParameters.TurnServer);

            var configuration = new RTCConfiguration
            {
                //IceServers = iceServers,
                PeerIdentity = roomRequestParameters.RoomId
            };
            var peerConnection = WebRtcMiddleware.WebRtc.Window(_jsRuntime).RTCPeerConnection(configuration);

            peerConnection.OnConnectionStateChanged += (s, e) => 
            { 
            };
            peerConnection.OnDataChannel += (s, e) =>
            {
            };
            peerConnection.OnIceCandidate += (s, e) =>
            {
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

            peerConnection.AddTrack(roomRequestParameters.LocalStream.GetVideoTracks().First(), 
                roomRequestParameters.LocalStream);
            peerConnection.AddTrack(roomRequestParameters.LocalStream.GetAudioTracks().First(), 
                roomRequestParameters.LocalStream);

            if (roomRequestParameters.IsInitiator)
            {
                var offerDescription = await peerConnection.CreateOffer();
                var offerDescriptionJson = offerDescription.ToJson();
                var sdp = offerDescription.Sdp;
                var type = offerDescription.Type; 


                await peerConnection.SetLocalDescription(offerDescription);

                var localDescription = peerConnection.LocalDescription;
                var localDescriptionJson = localDescription.ToJson();

                //// TODO Send offer message to signalling servewr to be broadcasted
                ///  _signallingServerClient.SendOfferMessage(localDescriptionJson);

            }


            return null;
        }

        public Task DisconnectRoomAsync(RoomRequestParameters roomRequestParameters)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(_signallingServerClient.CleanupAsync());
        }

        #region SignallingServerCallbacks
        public Task OnRoomJoined(string roomName, string pairUserName)
        {
            throw new NotImplementedException();
        }

        public Task OnRoomLeft(string roomName, string pairUserName)
        {
            throw new NotImplementedException();
        }

        public Task OnRoomStarted(string roomName, RTCIceServer[] iceServers)
        {
            throw new NotImplementedException();
        }

        public Task OnRoomStopped(string roomName)
        {
            throw new NotImplementedException();
        }

        public Task OnSdpOffered(string roomName, string pairUserName, string sdp)
        {
            throw new NotImplementedException();
        }

        public Task OnSdpAnswered(string roomName, string pairUserName, string sdp)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
