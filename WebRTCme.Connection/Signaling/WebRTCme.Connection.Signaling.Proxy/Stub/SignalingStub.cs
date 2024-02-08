using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;
using System;
using System.Net.Http;
using System.Reactive;
////using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilme;
using WebRTCme.Connection.Signaling;
////using Xamarin.Essentials;

namespace WebRTCme.Connection.Signaling.Proxy.Stub
{
    class SignalingStub : ISignalingServerApi
    {
        CancellationTokenSource _cts = new CancellationTokenSource();
        HubConnection _hubConnection;
        string _signallingServerBaseUrl;

        public event ISignalingServerNotify.PeerJoinedDelegateAsync PeerJoinedEventAsync;
        public event ISignalingServerNotify.PeerLeftDelegateAsync PeerLeftEventAsync;
        public event ISignalingServerNotify.PeerSdpAsyncDelegateAsync PeerSdpEventAsync;
        public event ISignalingServerNotify.PeerIceAsyncDelegateAsync PeerIceEventAsync;
        public event ISignalingServerNotify.PeerMediaAsyncDelegateAsync PeerMediaEventAsync;

        async Task OnPeerJoinedAsync(Guid peerId, string peerName) =>
            await PeerJoinedEventAsync?.Invoke(peerId, peerName);

        async Task OnPeerLeftAsync(Guid peerId) =>
            await PeerLeftEventAsync?.Invoke(peerId);

        async Task OnPeerSdpAsync(Guid peerId, string peerName, string peerSdp) =>
            await PeerSdpEventAsync?.Invoke(peerId, peerName, peerSdp);

        public async Task OnPeerIceAsync(Guid peerId, string peerIce) => 
            await PeerIceEventAsync?.Invoke(peerId, peerIce);

        public async Task OnPeerMediaAsync(Guid peerId, bool videoMuted, bool audioMuted, bool speaking) =>
            await PeerMediaEventAsync?.Invoke(peerId, videoMuted, audioMuted, speaking);


        public SignalingStub(IConfiguration configuration)
        {
            _signallingServerBaseUrl = configuration["SignalingServer:BaseUrl"];

            //// TODO: Bypass only for debugging with self signed certs (local IPs).
			////var bypassSslCertificateError = WebRTCme.DeviceInfoExt.IsAnroid;
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
                                clientHandler.ServerCertificateCustomValidationCallback =
                                        (sender, certificate, chain, sslPolicyErrors) => true;
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
            _hubConnection.On<Guid, string>(nameof(OnPeerJoinedAsync), OnPeerJoinedAsync);
            _hubConnection.On<Guid>(nameof(OnPeerLeftAsync), OnPeerLeftAsync);
            _hubConnection.On<Guid, string, string>(nameof(OnPeerSdpAsync), OnPeerSdpAsync);
            _hubConnection.On<Guid, string>(nameof(OnPeerIceAsync), OnPeerIceAsync);
            _hubConnection.On<Guid, bool, bool, bool>(nameof(OnPeerMediaAsync), OnPeerMediaAsync);

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


        public Task<Result<RTCIceServer[]>> GetIceServersAsync() =>
            _hubConnection.InvokeAsync<Result<RTCIceServer[]>>(nameof(GetIceServersAsync));

        public Task<Result<Unit>> JoinAsync(Guid id, string name, string room) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(JoinAsync), id, name, room);

        public Task<Result<Unit>> LeaveAsync(Guid id) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(LeaveAsync), id);

        public Task<Result<Unit>> SdpAsync(Guid peerId, string sdp) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(SdpAsync), peerId, sdp);

        public Task<Result<Unit>> IceAsync(Guid peerId, string ice) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(IceAsync), peerId, ice);

        public Task<Result<Unit>> MediaAsync(Guid id, bool videoMuted, bool audioMuted, bool speaking) =>
            _hubConnection.InvokeAsync<Result<Unit>>(nameof(MediaAsync), id, videoMuted, audioMuted, speaking);


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
