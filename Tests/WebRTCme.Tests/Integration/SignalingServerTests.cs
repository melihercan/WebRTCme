using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utilme;
using WebRTCme;
using WebRTCme.Connection.Signaling.Server.Hubs;
using WebRTCme.Connection.Signaling.Server.TurnServerProxies;

namespace WebRTCme.Tests.Integration;

/// <summary>
/// The mesh signalling server, hosted in process and driven by real SignalR clients.
/// </summary>
/// <remarks>
/// <para>
/// No browser, no device, no network: <see cref="TestServer"/> carries the transport in memory.
/// The clients speak MessagePack, which is what the production proxy uses, so a type that fails to
/// round-trip under that protocol fails here rather than on a phone.
/// </para>
/// <para>
/// Only <see cref="RoomHub"/> is mapped. The real <c>Startup</c> also brings up Blazor Server,
/// Razor pages, static files and HTTPS redirection, none of which this is testing and all of which
/// would need a content root and a host page to start.
/// </para>
/// <para>
/// <strong>The hub's room table is static.</strong> It is shared by every hub instance in the
/// process and therefore by every test in this class, and nothing clears it between them. So each
/// test uses its own room name and its own client ids - which is also why these tests cannot simply
/// be made parallel-safe by isolating the fixture.
/// </para>
/// <para>
/// <strong>Failures arrive as <see cref="HubException"/>, not as a Result.</strong>
/// <c>Result&lt;T&gt;</c> cannot carry one over MessagePack: it has no settable properties and one
/// public constructor taking the value, so deserialisation goes through that constructor and
/// <c>Status</c> and <c>ErrorMessage</c> revert to their defaults - a hub returning
/// <c>Error("...has already joined")</c> reached the client as <c>IsOk=true, Status=Ok,
/// ErrorMessage=null</c>. The hub therefore throws HubException, which SignalR propagates with its
/// message intact, and <c>SignalingStub</c> converts it back to a <c>Result.Error</c> so callers
/// keep the contract they had. These tests assert the raw wire behaviour, hence the
/// <c>HubException</c> rather than a Result.
/// </para>
/// </remarks>
public class SignalingServerTests : IAsyncLifetime
{
    IHost _host = null!;
    TestServer _server = null!;

    public async ValueTask InitializeAsync()
    {
        _host = await new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.ConfigureServices(services =>
                {
                    services.AddSignalR().AddMessagePackProtocol();
                    services.AddRouting();

                    // The TURN side, wired directly. Startup now picks the proxy from
                    // SignalingServer:TurnServer and resolves ITurnServerProxy through the factory
                    // - see issue #42 - and this mirrors the result rather than the mechanism,
                    // because what these tests exercise is RoomHub, not the selection. StunOnly is
                    // the default there and needs nothing external.
                    services.AddSingleton<TurnServerProxyFactory>();
                    services
                        .AddSingleton<StunOnlyProxy>()
                        .AddSingleton<ITurnServerProxy, StunOnlyProxy>(sp => sp.GetRequiredService<StunOnlyProxy>());
                });
                web.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapHub<RoomHub>("/roomhub"));
                });
            })
            .StartAsync(Ct);

        _server = _host.GetTestServer();
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    /// <summary>
    /// A client on the in-memory transport. Long polling rather than websockets, because that is
    /// what rides over TestServer's message handler without a socket to bind.
    /// </summary>
    HubConnection Client()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_server.BaseAddress, "roomhub"), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _server.CreateHandler();
            })
            .AddMessagePackProtocol()
            .Build();

        return connection;
    }

    /// <summary>
    /// The test's cancellation token, passed to everything that takes one so a cancelled run stops
    /// promptly rather than sitting in a hub call. Shortened to keep the assertions readable.
    /// </summary>
    static CancellationToken Ct => TestContext.Current.CancellationToken;

    static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    /// <summary>Waits for a notification, so a test fails as a timeout rather than hanging.</summary>
    static async Task<T> Awaited<T>(TaskCompletionSource<T> source)
    {
        var completed = await Task.WhenAny(source.Task, Task.Delay(Patience, Ct));
        completed.Should().BeSameAs(source.Task, "the notification should have arrived by now");
        return await source.Task;
    }

    [Fact]
    public async Task A_peer_can_join_a_room()
    {
        await using var peer = Client();
        await peer.StartAsync(Ct);

        var result = await peer.InvokeAsync<Result<Utilme.Unit>>(
            "JoinAsync", Guid.NewGuid(), "alice", $"room-{Guid.NewGuid():N}", Ct);

        result.IsOk.Should().BeTrue(result.ErrorMessage);
    }

    /// <summary>
    /// The id is the identity, so the same one joining twice is refused. This is also what made the
    /// missing disconnect handler so damaging: a client reconnecting after a dropped transport was
    /// turned away by its own ghost.
    /// </summary>
    [Fact]
    public async Task The_same_id_cannot_join_twice()
    {
        var room = $"room-{Guid.NewGuid():N}";
        var id = Guid.NewGuid();

        await using var peer = Client();
        await peer.StartAsync(Ct);

        (await peer.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", id, "alice", room, Ct)).IsOk.Should().BeTrue();

        var again = async () =>
            await peer.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", id, "alice", room, Ct);

        (await again.Should().ThrowAsync<HubException>())
            .WithMessage("*already joined*", "the server's own message must survive the trip");
    }

    [Fact]
    public async Task The_first_peer_in_a_room_is_told_about_the_second()
    {
        var room = $"room-{Guid.NewGuid():N}";
        var joined = new TaskCompletionSource<(Guid Id, string Name)>();

        await using var alice = Client();
        alice.On<Guid, string>("OnPeerJoinedAsync", (id, name) => joined.TrySetResult((id, name)));
        await alice.StartAsync(Ct);
        await alice.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", Guid.NewGuid(), "alice", room, Ct);

        var bobId = Guid.NewGuid();
        await using var bob = Client();
        await bob.StartAsync(Ct);
        await bob.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", bobId, "bob", room, Ct);

        var (id, name) = await Awaited(joined);
        id.Should().Be(bobId);
        name.Should().Be("bob");
    }

    /// <summary>
    /// The peer who creates the room has nobody to be told about, and must not be notified of its
    /// own arrival - a peer that announced itself to itself would appear twice in every roster.
    /// </summary>
    [Fact]
    public async Task A_peer_is_not_told_about_its_own_arrival()
    {
        var room = $"room-{Guid.NewGuid():N}";
        var notified = new TaskCompletionSource<Guid>();

        await using var alice = Client();
        alice.On<Guid, string>("OnPeerJoinedAsync", (id, _) => notified.TrySetResult(id));
        await alice.StartAsync(Ct);

        await alice.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", Guid.NewGuid(), "alice", room, Ct);

        var completed = await Task.WhenAny(notified.Task, Task.Delay(TimeSpan.FromSeconds(1), Ct));
        completed.Should().NotBeSameAs(notified.Task, "the room's first peer has nobody to hear about");
    }

    [Fact]
    public async Task Leaving_tells_the_other_peers()
    {
        var room = $"room-{Guid.NewGuid():N}";
        var left = new TaskCompletionSource<Guid>();

        await using var alice = Client();
        alice.On<Guid>("OnPeerLeftAsync", id => left.TrySetResult(id));
        await alice.StartAsync(Ct);
        await alice.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", Guid.NewGuid(), "alice", room, Ct);

        var bobId = Guid.NewGuid();
        await using var bob = Client();
        await bob.StartAsync(Ct);
        await bob.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", bobId, "bob", room, Ct);

        (await bob.InvokeAsync<Result<Utilme.Unit>>("LeaveAsync", bobId, Ct)).IsOk.Should().BeTrue();

        (await Awaited(left)).Should().Be(bobId);
    }

    /// <summary>
    /// A regression test with a measurement behind it. Until <c>OnDisconnectedAsync</c> was added,
    /// the only way out of a room was calling LeaveAsync politely - so a reloaded tab, a killed app
    /// or a phone losing wifi left the peer in the room for the life of the process. Measured on
    /// 2026-09-12 by reloading a tab mid-call: the Android peer kept encoding and sending a full
    /// camera stream to the tab that had gone, 4763 frames and climbing.
    /// <para>
    /// Here the transport is simply stopped, with no LeaveAsync, and the other peer must still be
    /// told.
    /// </para>
    /// </summary>
    [Fact]
    public async Task A_dropped_connection_is_treated_as_the_peer_leaving()
    {
        var room = $"room-{Guid.NewGuid():N}";
        var left = new TaskCompletionSource<Guid>();

        await using var alice = Client();
        alice.On<Guid>("OnPeerLeftAsync", id => left.TrySetResult(id));
        await alice.StartAsync(Ct);
        await alice.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", Guid.NewGuid(), "alice", room, Ct);

        var bobId = Guid.NewGuid();
        var bob = Client();
        await bob.StartAsync(Ct);
        await bob.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", bobId, "bob", room, Ct);

        // No LeaveAsync: the transport just goes away, the way a closed tab does.
        await bob.DisposeAsync();

        (await Awaited(left)).Should().Be(bobId);
    }

    /// <summary>
    /// A peer that left and rejoins with the same id must be let back in - which only works because
    /// the disconnect actually removed it. This is the "works until you refresh" shape of bug.
    /// </summary>
    [Fact]
    public async Task A_peer_can_rejoin_with_the_same_id_after_dropping()
    {
        var room = $"room-{Guid.NewGuid():N}";
        var id = Guid.NewGuid();

        // Somebody else has to be in the room, or it is removed when the last client goes and the
        // rejoin proves nothing about ghosts.
        await using var alice = Client();
        await alice.StartAsync(Ct);
        await alice.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", Guid.NewGuid(), "alice", room, Ct);

        var left = new TaskCompletionSource<Guid>();
        alice.On<Guid>("OnPeerLeftAsync", peer => left.TrySetResult(peer));

        var bob = Client();
        await bob.StartAsync(Ct);
        await bob.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", id, "bob", room, Ct);
        await bob.DisposeAsync();

        await Awaited(left);

        await using var bobAgain = Client();
        await bobAgain.StartAsync(Ct);
        var rejoin = await bobAgain.InvokeAsync<Result<Utilme.Unit>>("JoinAsync", id, "bob", room, Ct);

        rejoin.IsOk.Should().BeTrue(rejoin.ErrorMessage);
    }

    [Fact]
    public async Task Leaving_a_room_nobody_is_in_is_an_error_rather_than_a_crash()
    {
        await using var peer = Client();
        await peer.StartAsync(Ct);

        var leave = async () =>
            await peer.InvokeAsync<Result<Utilme.Unit>>("LeaveAsync", Guid.NewGuid(), Ct);

        (await leave.Should().ThrowAsync<HubException>()).WithMessage("*no user found*");
    }

    [Fact]
    public async Task The_server_hands_out_ice_servers()
    {
        await using var peer = Client();
        await peer.StartAsync(Ct);

        var result = await peer.InvokeAsync<Result<RTCIceServer[]>>("GetIceServersAsync", Ct);

        result.IsOk.Should().BeTrue(result.ErrorMessage);
        result.Value.Should().NotBeNullOrEmpty("a peer cannot gather candidates without at least a STUN server");
    }
}
