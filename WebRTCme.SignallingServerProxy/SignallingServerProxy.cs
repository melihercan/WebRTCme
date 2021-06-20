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
        CancellationTokenSource _cts = new CancellationTokenSource();
        HubConnection _hubConnection;
        string _signallingServerBaseUrl;

        public event ISignallingServerProxy.JoinedOrLeftCallbackHandler OnPeerJoinedAsyncEvent;
        public event ISignallingServerProxy.JoinedOrLeftCallbackHandler OnPeerLeftAsyncEvent;
        public event ISignallingServerProxy.SdpOrIceCallbackHandler OnPeerSdpAsyncEvent;
        public event ISignallingServerProxy.SdpOrIceCallbackHandler OnPeerIceAsyncEvent;

        async Task OnPeerJoinedAsync(string turnServerName, string roomName, string peerUserName) => 
            await OnPeerJoinedAsyncEvent?.Invoke(turnServerName, roomName, peerUserName);

        async Task OnPeerLeftAsync(string turnServerName, string roomName, string peerUserName) => 
            await OnPeerLeftAsyncEvent?.Invoke(turnServerName, roomName, peerUserName);

        async Task OnPeerSdpAsync(string turnServerName, string roomName, string peerUserName, string peerSdp) => 
            await OnPeerSdpAsyncEvent?.Invoke(turnServerName, roomName, peerUserName, peerSdp);

        public async Task OnPeerIceCandidateAsync(string turnServerName, string roomName, string peerUserName,
            string peerIce) => await OnPeerIceAsyncEvent?.Invoke(turnServerName, roomName, peerUserName, peerIce);


        public SignallingServerProxy(IConfiguration configuration)
        {
            _signallingServerBaseUrl = configuration["SignallingServer:BaseUrl"];

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
            _hubConnection.On<string, string, string>(nameof(OnPeerJoinedAsync), OnPeerJoinedAsync);
            _hubConnection.On<string, string, string>(nameof(OnPeerLeftAsync), OnPeerLeftAsync);
            _hubConnection.On<string, string, string, string>(nameof(OnPeerSdpAsync), OnPeerSdpAsync);
            _hubConnection.On<string, string, string, string>(nameof(OnPeerIceCandidateAsync), OnPeerIceCandidateAsync);

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

        public Task<(SignallingServerResult, string[])> GetTurnServerNamesAsync() => 
            _hubConnection.InvokeAsync<(SignallingServerResult, string[])>(nameof(GetTurnServerNamesAsync));

        public Task<(SignallingServerResult, RTCIceServer[])> GetIceServersAsync(string turnServerName) =>
            _hubConnection.InvokeAsync< (SignallingServerResult, RTCIceServer[])>(nameof(GetIceServersAsync), 
                turnServerName);

        public Task<SignallingServerResult> JoinRoomAsync(string turnServerName, string roomName, string userName) =>
            _hubConnection.InvokeAsync<SignallingServerResult>(nameof(JoinRoomAsync), turnServerName, roomName, 
                userName);

        public Task<SignallingServerResult> LeaveRoomAsync(string turnServerName, string roomName, string userName) =>
            _hubConnection.InvokeAsync<SignallingServerResult>(nameof(LeaveRoomAsync), turnServerName, roomName, 
                userName);

        public Task<SignallingServerResult> SdpAsync(string turnServerName, string roomName, string peerUserName,
            string sdp) =>
                _hubConnection.InvokeAsync<SignallingServerResult>(nameof(SdpAsync), turnServerName, roomName, 
                    peerUserName, sdp);

        public Task<SignallingServerResult> IceCandidateAsync(string turnServerName, string roomName, string peerUserName,
            string ice) =>
                _hubConnection.InvokeAsync<SignallingServerResult>(nameof(IceCandidateAsync), turnServerName, roomName, 
                    peerUserName, ice);

        Task HubConnection_Closed(Exception arg)
        {
            if (arg != null)
            {
                // arg null means connection is closed either by client or server and NOT due to error or exception.
                // Start connection again without awaiting in error or exception case.
                _ = ConnectWithRetryAsync();
            }
            
            return Task.CompletedTask;
        }

        async Task ConnectWithRetryAsync()
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
                    // Failed to connect, trying again in TimeoutMs.
                    await Task.Delay(TimeoutMs);
                }
            }
        }
    }
}
