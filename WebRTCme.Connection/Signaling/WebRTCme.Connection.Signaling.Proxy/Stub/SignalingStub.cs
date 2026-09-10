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
        // Long enough for a WebSocket closing handshake on a slow link, short enough that a server
        // that has already gone away cannot hang the app's teardown.
        static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);

        // Serialises connect against disconnect: the stub is a singleton reused across calls, so a
        // join arriving while the previous call is still closing must not race it.
        readonly SemaphoreSlim _connectGate = new SemaphoreSlim(1, 1);

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

            // Start connecting without waiting; a join will await EnsureConnectedAsync anyway.
            _ = EnsureConnectedAsync();
        }

        public async Task EnsureConnectedAsync()
        {
            if (_hubConnection.State == HubConnectionState.Connected)
                return;

            await _connectGate.WaitAsync(_cts.Token);
            try
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                    await ConnectWithRetryAsync();
            }
            finally
            {
                _connectGate.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
                return;

            await _connectGate.WaitAsync();
            try
            {
                await StopGracefullyAsync();
            }
            finally
            {
                _connectGate.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            // Cancel first, so ConnectWithRetryAsync stops retrying and cannot reconnect underneath
            // the shutdown below. DisconnectAsync deliberately does not do this - it has to leave
            // the stub reusable, since it is a singleton and a later call will join again.
            _cts.Cancel();

            await StopGracefullyAsync();
            await _hubConnection.DisposeAsync();
        }

        async Task StopGracefullyAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
                return;

            try
            {
                // Deliberately NOT _cts.Token. In DisposeAsync it has just been cancelled, and
                // StopAsync given an already-cancelled token aborts immediately rather than
                // performing the WebSocket closing handshake - so the close never happened and the
                // catch below hid it.
                using var closeCts = new CancellationTokenSource(CloseTimeout);
                await _hubConnection.StopAsync(closeCts.Token);
            }
            catch
            {
                // A close that fails or times out is not worth failing teardown over: LeaveAsync
                // has already removed the peer, so the server has nothing stale to hold on to.
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
