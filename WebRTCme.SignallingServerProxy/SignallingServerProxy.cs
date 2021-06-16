using Ardalis.Result;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WebRTCme.SignallingServerProxy
{
    public class SignallingServerProxy : ISignallingServerProxy
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private HubConnection _hubConnection;

        private string _signallingServerBaseUrl;

        public SignallingServerProxy(IConfiguration configuration, //string signallingServerBaseUrl,
            ISignallingServerCallbacks signallingServerCallbacks)
        {
            _signallingServerBaseUrl = configuration["SignallingServer:BaseUrl"];// signallingServerBaseUrl;

            var bypassSslCertificateError = DeviceInfo.Platform == DevicePlatform.Android;

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
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel./*Error*/Debug);
                })
                //// iOS has problems with MessagePack:
                //// https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/signalr/messagepackhubprotocol.md   
                //.If(DeviceInfo.Platform != DevicePlatform.iOS, builder => builder.AddMessagePackProtocol())
                //// Workaround: tick 'Enable the Mono interpreter' option (unticked for release build)
                .AddMessagePackProtocol()
                .Build();

            // Register callback handlers invoked by server hub.
            _hubConnection.On<string, string, string>(nameof(signallingServerCallbacks.OnPeerJoinedAsync),
                signallingServerCallbacks.OnPeerJoinedAsync);
            _hubConnection.On<string, string, string>(nameof(signallingServerCallbacks.OnPeerLeftAsync),
                signallingServerCallbacks.OnPeerLeftAsync);
            _hubConnection.On<string, string, string, string>(nameof(signallingServerCallbacks.OnPeerSdpAsync),
                signallingServerCallbacks.OnPeerSdpAsync);
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

        public Task<Result<Unit>> JoinRoomAsync(string turnServerName, string roomName, string userName) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(JoinRoomAsync), turnServerName, roomName, userName);

        public Task<Result<Unit>> LeaveRoomAsync(string turnServerName, string roomName, string userName) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(LeaveRoomAsync), turnServerName, roomName, userName);

        public Task<Result<Unit>> SdpAsync(string turnServerName, string roomName, string peerUserName,
            string sdp) =>
                _hubConnection.InvokeAsync<Result<Unit>>(nameof(SdpAsync), turnServerName, roomName, peerUserName,
                    sdp);

        public Task<Result<Unit>> IceCandidateAsync(string turnServerName, string roomName, string peerUserName,
            string ice) =>
                _hubConnection.InvokeAsync<Result<Unit>>(nameof(IceCandidateAsync), turnServerName, roomName, peerUserName, ice);

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
                    break;
                }
                catch when (_cts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch 
                {
                    // Failed to connect, trying again in TimeoutMs if not overriden by keepTrying flag.
                    await Task.Delay(TimeoutMs);
                }
            }
        }
    }
}
