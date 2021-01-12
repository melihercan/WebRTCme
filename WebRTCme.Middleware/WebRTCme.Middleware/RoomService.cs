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
    internal class RoomService : IRoomService
    {
        private ISignallingServerClient _signallingServerClient;
        private IJSRuntime _jsRuntime;

        static public async Task<IRoomService> CreateAsync(string signallingServerBaseUrl, IJSRuntime jsRuntime = null)
        {
            var self = new RoomService();
            self._signallingServerClient = SignallingServerClientFactory.Create(signallingServerBaseUrl);
            await self._signallingServerClient.InitializeAsync();
            self._jsRuntime = jsRuntime;
            return self;
        }

        private RoomService() { }

        public async Task<IMediaStream> ConnectRoomAsync(RoomRequestParameters roomRequestParameters)
        {
            /*var iceServers =*/ await (roomRequestParameters.IsJoin ?
                _signallingServerClient.JoinRoomAsync(roomRequestParameters.RoomId, roomRequestParameters.UserId) :
                _signallingServerClient.StartRoomAsync(roomRequestParameters.RoomId, roomRequestParameters.UserId, roomRequestParameters.TurnServer));

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

            peerConnection.AddTrack(roomRequestParameters.LocalStream.GetVideoTracks().First(), roomRequestParameters.LocalStream);
            peerConnection.AddTrack(roomRequestParameters.LocalStream.GetAudioTracks().First(), roomRequestParameters.LocalStream);

            if (!roomRequestParameters.IsJoin)
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

        public ValueTask DisposeAsync()
        {
            return new ValueTask(_signallingServerClient.CleanupAsync());
        }

    }
}
