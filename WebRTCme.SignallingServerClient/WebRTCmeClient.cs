using Ardalis.Result;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
    internal class WebRTCmeClient : ISignallingServerClient//, ISignallingServerCallbacks
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private HubConnection _hubConnection;

        private readonly string _signallingServerBaseUrl;

        public WebRTCmeClient(string signallingServerBaseUrl)
        {
            _signallingServerBaseUrl = signallingServerBaseUrl;
        }

        public Task InitializeAsync(ISignallingServerCallbacks signallingServerCallbacks)
        {
            bool bypassSslCertificateError = DeviceInfo.Platform == DevicePlatform.Android;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_signallingServerBaseUrl + "/roomhub", (opts) =>
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
            _hubConnection.On<string, RTCIceServer[]>("OnRoomStarted", signallingServerCallbacks.OnRoomStarted);
            _hubConnection.On<string>("OnRoomStopped", signallingServerCallbacks.OnRoomStopped);
            _hubConnection.On<string, string>("OnPeerJoined", signallingServerCallbacks.OnPeerJoined);
            _hubConnection.On<string, string>("OnPeerLeft", signallingServerCallbacks.OnPeerLeft);
            _hubConnection.On<string, string, string>("OnPeerSdpOffered", signallingServerCallbacks.OnPeerSdpOffered);
            _hubConnection.On<string, string, string>("OnPeerSdpAnswered", signallingServerCallbacks.OnPeerSdpAnswered);

            _hubConnection.Closed += HubConnection_Closed;

            // Start connection without waiting.
            _ = ConnectWithRetryAsync();

            return Task.CompletedTask;
        }

        public async Task CleanupAsync()
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

        public async Task JoinRoomAsync(string roomName, string userName)
        {
            var result = await _hubConnection.InvokeAsync<Result<Unit>>("JoinRoom", roomName, userName);
            if (result.Status != ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
        }

        public async Task LeaveRoomAsync(string roomName, string userName)
        {
            var result = await _hubConnection.InvokeAsync<Result<Unit>>("LeaveRoom", roomName, userName);
            if (result.Status != ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
        }

        public async Task StartRoomAsync(string roomName, string userName, TurnServer turnServer)
        {
            var result = await _hubConnection.InvokeAsync<Result<Unit>>("StartRoom", roomName, userName, turnServer);
            if (result.Status != ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
        }

        public async Task StopRoomAsync(string roomName, string userName)
        {
            var result = await _hubConnection.InvokeAsync<Result<Unit>>("StopRoom", roomName, userName);
            if (result.Status != ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
        }


        public async Task OfferSdpAsync(string roomName, string pairUserName, string sdp)
        {
            var result = await _hubConnection.InvokeAsync<Result<Unit>>("OfferSdp", roomName, pairUserName, sdp);
            if (result.Status != ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
        }

        public async Task AnswerSdpAsync(string roomName, string pairUserName, string sdp)
        {
            var result = await _hubConnection.InvokeAsync<Result<Unit>>("AnswerSdp", roomName, pairUserName, sdp);
            if (result.Status != ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
        }

        public async Task IceCandidateAsync(string roomName, string pairUserName, string ice)
        {
            var result = await _hubConnection.InvokeAsync<Result<Unit>>("IceCandidate", roomName, pairUserName, ice);
            if (result.Status != ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
        }

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
