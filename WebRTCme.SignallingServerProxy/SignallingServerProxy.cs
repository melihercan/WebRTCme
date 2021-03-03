using Ardalis.Result;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
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
using WebRTCme;
using WebRTCme.SignallingServerProxy;
using Xamarin.Essentials;

namespace WebRTCme.SignallingServerProxy
{
    public class SignallingServerProxy : ISignallingServerProxy
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private HubConnection _hubConnection;

        private string _signallingServerBaseUrl;

        public SignallingServerProxy(string signallingServerBaseUrl,
                    ISignallingServerCallbacks signallingServerCallbacks)
        {
            _signallingServerBaseUrl = signallingServerBaseUrl;

            var bypassSslCertificateError = DeviceInfo.Platform == DevicePlatform.Android;

            _hubConnection = new HubConnectionBuilder()
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
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Error/*Debug*/);
                })
                .AddMessagePackProtocol()
                .Build();

            // Register callback handlers invoked by server hub.
            _hubConnection.On<string, string, string>(nameof(signallingServerCallbacks.OnPeerJoinedAsync),
                signallingServerCallbacks.OnPeerJoinedAsync);
            _hubConnection.On<string, string, string>(nameof(signallingServerCallbacks.OnPeerLeftAsync),
                signallingServerCallbacks.OnPeerLeftAsync);
            _hubConnection.On<string, string, string, string>(nameof(signallingServerCallbacks.OnPeerSdpOfferedAsync),
                signallingServerCallbacks.OnPeerSdpOfferedAsync);
            _hubConnection.On<string, string, string, string>(nameof(signallingServerCallbacks.OnPeerSdpAnsweredAsync),
                signallingServerCallbacks.OnPeerSdpAnsweredAsync);
            _hubConnection.On<string, string, string, string>(nameof(signallingServerCallbacks.OnPeerIceCandidateAsync),
                signallingServerCallbacks.OnPeerIceCandidateAsync);

            _hubConnection.Closed += HubConnection_Closed;

            // Start connection without waiting.
            _ = ConnectWithRetryAsync();
        }
        public async ValueTask DisposeAsync()
        {
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

        public Task<Result<string[]>> GetTurnServerNamesAsync() => 
            _hubConnection.InvokeAsync<Result<string[]>>(nameof(GetTurnServerNamesAsync));

        public Task<Result<RTCIceServer[]>> GetIceServersAsync(string turnServerName) =>
            _hubConnection.InvokeAsync<Result<RTCIceServer[]>>(nameof(GetIceServersAsync), turnServerName);

        //public Task<Result<Unit>> ReserveRoomAsync(string turnServerName, string roomName, string adminUserName,
        //    string[] participantUserNames) =>
        //        _hubConnection.InvokeAsync<Result<Unit>>(nameof(ReserveRoomAsync), turnServerName, roomName, 
        //            adminUserName, participantUserNames);

        //public Task<Result<Unit>> FreeRoomAsync(string turnServerName, string roomName, string adminUserName) =>
        //    _hubConnection.InvokeAsync<Result<Unit>>(nameof(FreeRoomAsync), turnServerName, roomName, adminUserName);

        //public Task<Result<Unit>> AddParticipantAsync(string turnServerName, string roomName, 
        //    string participantUserName) =>
        //        _hubConnection.InvokeAsync<Result<Unit>>(nameof(AddParticipantAsync), turnServerName, roomName, 
        //            participantUserName);

        //public Task<Result<Unit>> RemoveParticipantAsync(string turnServerName, string roomName,
        //    string participantUserName) =>
        //        _hubConnection.InvokeAsync<Result<Unit>>(nameof(RemoveParticipantAsync), turnServerName, roomName,
        //            participantUserName);

        public Task<Result<Unit>> JoinRoomAsync(string turnServerName, string roomName, string userName) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(JoinRoomAsync), turnServerName, roomName, userName);

        public Task<Result<Unit>> LeaveRoomAsync(string turnServerName, string roomName, string userName) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(LeaveRoomAsync), turnServerName, roomName, userName);

        public Task<Result<Unit>> OfferSdpAsync(string turnServerName, string roomName, string pairUserName, 
            string sdp) =>
                _hubConnection.InvokeAsync<Result<Unit>>(nameof(OfferSdpAsync), turnServerName, roomName, pairUserName, 
                    sdp);

        public Task<Result<Unit>> AnswerSdpAsync(string turnServerName, string roomName, string pairUserName,
            string sdp) =>
                _hubConnection.InvokeAsync<Result<Unit>>(nameof(AnswerSdpAsync), turnServerName, roomName, pairUserName, 
                    sdp);

        public Task<Result<Unit>> IceCandidateAsync(string turnServerName, string roomName, string pairUserName,
            string ice) =>
                _hubConnection.InvokeAsync<Result<Unit>>(nameof(IceCandidateAsync), turnServerName, roomName, pairUserName, ice);

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
    }
}
