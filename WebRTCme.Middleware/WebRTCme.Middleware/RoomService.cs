using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private CancellationTokenSource _cts = new();

        static public async Task<IRoomService> CreateAsync(string signallingServerBaseUrl, IJSRuntime jsRuntime = null)
        {
            var self = new RoomService();
            self._signallingServerClient = SignallingServerClientFactory.Create(signallingServerBaseUrl);
            await self._signallingServerClient.InitializeAsync(self);
            self._jsRuntime = jsRuntime;
            _ = Task.Run(async () => await self.SignallingServerCallbacksHandler(self._cts.Token));
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

            await _signallingServerClient.JoinRoomAsync(roomRequestParameters.RoomName, roomRequestParameters.UserName);

            if (roomRequestParameters.IsInitiator)
                await _signallingServerClient.StartRoomAsync(roomRequestParameters.RoomName,
                    roomRequestParameters.UserName, roomRequestParameters.TurnServer);


            roomContext.RoomState = RoomState.Connecting;

            return await roomContext.ConnectTcs.Task;
        }

        private async Task SignallingServerCallbacksHandler(CancellationToken ct)
        {
            try
            {
                var taskArray = new Task<RoomResponseParameters>[]
                {
                RoomContext.RoomStarted.Task,
                RoomContext.RoomStopped.Task,
                RoomContext.PeerJoined.Task,
                RoomContext.PeerLeft.Task,
                RoomContext.PeerSdpOffered.Task,
                RoomContext.PeerSdpAnswered.Task
                };

                while (!ct.IsCancellationRequested)
                {
                    var completedTaskIndex = Task.WaitAny(taskArray, ct);
                    switch (completedTaskIndex)
                    {
                        case 0: await RoomStarted(); break;
                        case 1: await RoomStopped(); break;
                        case 2: await PeerJoined(); break;
                        case 3: await PeerLeft(); break;
                        case 4: await PeerSdpOffered(); break;
                        case 5: await PeerSdpAnswered(); break;
                    }
                }
            }
            catch (Exception ex)
            {
                await FatalErrorAsync(ex.Message);
            }

            async Task RoomStarted()
            {
                var response = RoomContext.RoomStarted.Task.Result;
                var roomContext = RoomContextFromName(response.RoomName);
                try
                {
                    if (roomContext.RoomState == RoomState.Error)
                        return;
                    if (roomContext.RoomState != RoomState.Connecting)
                        throw new Exception($"Room {roomContext.RoomRequestParameters.RoomName} " +
                            $"is in wrong state {roomContext.RoomState}");

                    var configuration = new RTCConfiguration
                    {
                        IceServers = response.IceServers,
                        PeerIdentity = response.RoomName
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

                    peerConnection.AddTrack(roomContext.RoomRequestParameters.LocalStream.GetVideoTracks().First(),
                        roomContext.RoomRequestParameters.LocalStream);
                    peerConnection.AddTrack(roomContext.RoomRequestParameters.LocalStream.GetAudioTracks().First(),
                        roomContext.RoomRequestParameters.LocalStream);



                }
                catch (Exception ex)
                {
                    await AbortConnectionAsync(roomContext, ex.Message);
                }
            }

            async Task RoomStopped()
            {
                var response = RoomContext.RoomStopped.Task.Result;
                var roomContext = RoomContextFromName(response.RoomName);
                try
                {
                    if (roomContext.RoomState == RoomState.Error)
                        return;

                }
                catch (Exception ex)
                {

                }
            }

            async Task PeerJoined()
            {
                var response = RoomContext.PeerJoined.Task.Result;
                var roomContext = RoomContextFromName(response.RoomName);
                try
                {
                    if (roomContext.RoomState == RoomState.Error)
                        return;
                }
                catch (Exception ex)
                {

                }
            }

            async Task PeerLeft()
            {
                var response = RoomContext.PeerLeft.Task.Result;
                var roomContext = RoomContextFromName(response.RoomName);
                try
                {
                    if (roomContext.RoomState == RoomState.Error)
                        return;

                }
                catch (Exception ex)
                {

                }
            }


            async Task PeerSdpOffered()
            {
                var response = RoomContext.PeerSdpOffered.Task.Result;
                var roomContext = RoomContextFromName(response.RoomName);
                try
                {
                    if (roomContext.RoomState == RoomState.Error)
                        return;

                }
                catch (Exception ex)
                {

                }
            }

            async Task PeerSdpAnswered()
            {
                var response = RoomContext.PeerSdpAnswered.Task.Result;
                var roomContext = RoomContextFromName(response.RoomName);
                try
                {
                    if (roomContext.RoomState == RoomState.Error)
                        return;

                }
                catch (Exception ex)
                {

                }
            }

            async Task AbortConnectionAsync(RoomContext roomContext, string message)
            {
                roomContext.RoomState = RoomState.Error;
                await _signallingServerClient.LeaveRoomAsync(roomContext.RoomRequestParameters.RoomName,
                    roomContext.RoomRequestParameters.UserName);
                if (roomContext.RoomRequestParameters.IsInitiator)
                    await _signallingServerClient.StopRoomAsync(roomContext.RoomRequestParameters.RoomName,
                        roomContext.RoomRequestParameters.UserName);
            }

            async Task FatalErrorAsync(string message)
            {
                //// TODO: what???
                ///
                await Task.CompletedTask;
            }
#if false
            roomContext.RoomState = RoomState.Connecting;

            var configuration = new RTCConfiguration
            {
                //IceServers = iceServers,
                PeerIdentity = roomRequestParameters.RoomName
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

            }
#endif

        }

        public Task DisconnectRoomAsync(RoomRequestParameters roomRequestParameters)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(_signallingServerClient.CleanupAsync());
        }

        private RoomContext RoomContextFromName(string roomName) =>
            _roomContexts.FirstOrDefault(context => context.RoomRequestParameters.RoomName == roomName);



        #region SignallingServerCallbacks
        public Task OnRoomStarted(string roomName, RTCIceServer[] iceServers)
        {
            throw new NotImplementedException();
        }

        public Task OnRoomStopped(string roomName)
        {
            throw new NotImplementedException();
        }

        public Task OnPeerJoined(string roomName, string peerUserName)
        {
            throw new NotImplementedException();
        }

        public Task OnPeerLeft(string roomName, string peerUserName)
        {
            throw new NotImplementedException();
        }


        public Task OnPeerSdpOffered(string roomName, string peerUserName, string peerSdp)
        {
            throw new NotImplementedException();
        }

        public Task OnPeerSdpAnswered(string roomName, string peerUserName, string peerSdp)
        {
            throw new NotImplementedException();
        }

#endregion


    }
}
