using Ardalis.Result;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WebRTCme.SignallingServerClient
{
    internal class WebRTCmeClient : ISignallingServerClient
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private HubConnection _hubConnection;

        private string _signallingServerBaseUrl;

        private WebRTCmeClient()
        {
        }

        public static Task<ISignallingServerClient> CreateAsync(string signallingServerBaseUrl, 
            ISignallingServerCallbacks signallingServerCallbacks)
        {
            var self = new WebRTCmeClient();

            self._signallingServerBaseUrl = signallingServerBaseUrl;

            bool bypassSslCertificateError = DeviceInfo.Platform == DevicePlatform.Android;

            self._hubConnection = new HubConnectionBuilder()
                .WithUrl(signallingServerBaseUrl + "/roomhub", (opts) =>
                {
                    if (bypassSslCertificateError)
                    {
                        opts.HttpMessageHandlerFactory = (message) =>
                        {
                            if (message is HttpClientHandler clientHandler)
                            // Bypass SSL certificate.
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                    (sender, certificate, chain, sslPolicyErrors) => { return true; };
                            return message;
                        };
                    }
                })
                .ConfigureLogging(logging =>
                {
                    // Log to output window.
                    logging.AddDebug();

                    // This will set ALL logging to Debug level
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .AddMessagePackProtocol()
                .Build();

            // Register callback handlers invoked by server hub.
            self._hubConnection.On<string, string, string>("OnPeerJoined", signallingServerCallbacks.OnPeerJoined);
            self._hubConnection.On<string, string, string>("OnPeerLeft", signallingServerCallbacks.OnPeerLeft);
            self._hubConnection.On<string, string, string, string>("OnPeerSdpOffered", 
                signallingServerCallbacks.OnPeerSdpOffered);
            self._hubConnection.On<string, string, string, string>("OnPeerSdpAnswered", 
                signallingServerCallbacks.OnPeerSdpAnswered);
            self._hubConnection.On<string, string, string, string>("OnPeerIceCandidate", 
                signallingServerCallbacks.OnPeerIceCandidate);

            self._hubConnection.Closed += self.HubConnection_Closed;

            self.SyncInitialize();

            // Start connection without waiting.
            _ = self.ConnectWithRetryAsync();


            return Task.FromResult(self as ISignallingServerClient);
        }

        public async ValueTask DisposeAsync()
        {
            SyncCleanup();
            _cts.Cancel();
            if (_hubConnection.State != HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StopAsync(_cts.Token);
                }
                catch { }
            }
        }

        public Task<Result<string[]>> GetTurnServerNames() => 
            _hubConnection.InvokeAsync<Result<string[]>>("GetTurnServerNames");

        public Task<Result<RTCIceServer[]>> GetIceServers(string turnServerName) =>
            _hubConnection.InvokeAsync<Result<RTCIceServer[]>>("GetIceServers", turnServerName);

        public Task<Result<Unit>> ReserveRoom(string turnServerName, string roomName, string adminUserName,
            string[] participantUserNames) =>
                _hubConnection.InvokeAsync<Result<Unit>>("ReserveRoom", turnServerName, roomName, adminUserName,
                    participantUserNames);

        public Task<Result<Unit>> FreeRoom(string turnServerName, string roomName, string adminUserName) =>
            _hubConnection.InvokeAsync<Result<Unit>>("FreeRoom", turnServerName, roomName, adminUserName);

        public Task<Result<Unit>> AddParticipant(string turnServerName, string roomName, 
            string participantUserName) =>
                _hubConnection.InvokeAsync<Result<Unit>>("AddParticipant", turnServerName, roomName, 
                    participantUserName);

        public Task<Result<Unit>> RemoveParticipant(string turnServerName, string roomName,
            string participantUserName) =>
                _hubConnection.InvokeAsync<Result<Unit>>("RemoveParticipant", turnServerName, roomName,
                    participantUserName);

        public Task<Result<Unit>> JoinRoom(string turnServerName, string roomName, string userName) =>
            _hubConnection.InvokeAsync<Result<Unit>>("JoinRoom", turnServerName, roomName, userName);

        public Task<Result<Unit>> LeaveRoom(string turnServerName, string roomName, string userName) =>
            _hubConnection.InvokeAsync<Result<Unit>>("LeaveRoom", turnServerName, roomName, userName);

        public Task<Result<Unit>> OfferSdp(string turnServerName, string roomName, string pairUserName, 
            string sdp) =>
                _hubConnection.InvokeAsync<Result<Unit>>("OfferSdp", turnServerName, roomName, pairUserName, sdp);

        public Task<Result<Unit>> AnswerSdp(string turnServerName, string roomName, string pairUserName,
            string sdp) =>
                _hubConnection.InvokeAsync<Result<Unit>>("AnswerSdp", turnServerName, roomName, pairUserName, sdp);

        public Task<Result<Unit>> IceCandidate(string turnServerName, string roomName, string pairUserName,
            string ice) =>
                _hubConnection.InvokeAsync<Result<Unit>>("IceCandidate", turnServerName, roomName, pairUserName, ice);

        private Task HubConnection_Closed(Exception arg)
        {
            if (arg != null)
            {
                // arg null means connection is closed either by client or server and NOT due to error or exception.
                // Start connection again without awaiting in error or exception case.
                _ = ConnectWithRetryAsync();
            }
            
            return Task.CompletedTask;
        }

        private async Task ConnectWithRetryAsync()
        {
            const int TimeoutMs = 1000;

            // Keep trying until we can start or the token is canceled.
            while (true)
            {
                try
                {
                    await _hubConnection.StartAsync(_cts.Token);
                    Debug.WriteLine("#### Connected to Signalling Server...");
                    break;
                }
                catch when (_cts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("====> EXCEPTION: " + ex.Message);
                    ////throw ex;
                    // Failed to connect, trying again in TimeoutMs if not overriden by keepTrying flag.
                    await Task.Delay(TimeoutMs);
                }
            }
        }

        #region Sync 

        internal class Data
        {
            public string MethodName { get; set; }
            public string TurnServerName { get; set; }
            public string RoomName { get; set; }
            public string PairUserName { get; set; }
            public string SdpOrIce { get; set; }
        }

        private BlockingCollection<Data> _collection = new();
        private CancellationTokenSource _ctsSync = new(); 

        private void SyncInitialize()
        {
            try
            {
                // This will fail on BlazorWasm. Async calls are used for it.
                new Thread(ThreadExecute).Start();
            }
            catch
            { }
        }

        private void ThreadExecute(object arg)
        {

            while (!_ctsSync.Token.IsCancellationRequested)
            {
                Data data = null;
                try
                {
                    data = _collection.Take(_ctsSync.Token);
                }
                catch
                {  }

                if (data is not null)
                {
                    switch (data.MethodName)
                    {
                        case nameof(OfferSdpSync):
                            _ = OfferSdp(data.TurnServerName, data.RoomName, data.PairUserName, data.SdpOrIce)
                                .GetAwaiter().GetResult();
                            break;

                        case nameof(AnswerSdpSync):
                            _ = AnswerSdp(data.TurnServerName, data.RoomName, data.PairUserName, data.SdpOrIce)
                                .GetAwaiter().GetResult();
                            break;

                        case nameof(IceCandidateSync):
                            _ = IceCandidate(data.TurnServerName, data.RoomName, data.PairUserName, data.SdpOrIce)
                                .GetAwaiter().GetResult();
                            break;
                    }
                }
            }
        }

        private void SyncCleanup()
        {
            _ctsSync.Cancel();
        }

        public Result<Unit> OfferSdpSync(string turnServerName, string roomName, string pairUserName, string sdp)
        {
            _collection.Add(new Data 
            { 
                MethodName = nameof(OfferSdpSync),
                TurnServerName = turnServerName,
                RoomName = roomName,
                PairUserName = pairUserName,
                SdpOrIce = sdp
            });
            return Unit.Default;
        }

        public Result<Unit> AnswerSdpSync(string turnServerName, string roomName, string pairUserName, string sdp)
        {
            _collection.Add(new Data
            {
                MethodName = nameof(AnswerSdpSync),
                TurnServerName = turnServerName,
                RoomName = roomName,
                PairUserName = pairUserName,
                SdpOrIce = sdp
            });
            return Unit.Default;
        }

        public Result<Unit> IceCandidateSync(string turnServerName, string roomName, string pairUserName, string ice)
        {
            _collection.Add(new Data
            {
                MethodName = nameof(IceCandidateSync),
                TurnServerName = turnServerName,
                RoomName = roomName,
                PairUserName = pairUserName,
                SdpOrIce = ice
            });
            return Unit.Default;
        }

        #endregion
    }
}
