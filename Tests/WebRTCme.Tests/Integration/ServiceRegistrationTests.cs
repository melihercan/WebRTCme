using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebRTCme.Connection;
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.ClientWebSockets;
using WebRTCme.Connection.Signaling.Proxy;
using WebRTCme.Middleware;

namespace WebRTCme.Tests.Integration;

/// <summary>
/// What <c>AddMiddleware()</c> and the registrations beneath it put into a container.
/// </summary>
/// <remarks>
/// These assert the service <em>descriptors</em> rather than resolving everything, deliberately.
/// Most of this graph cannot be instantiated off a device - <c>LocalMediaStream</c> reaches through
/// to the platform's media devices in its constructor - but a registration silently dropped during
/// a refactor is exactly the kind of fault that then surfaces as a null reference three layers away
/// at runtime. The descriptors are checkable anywhere, and that is where the value is.
/// <para>
/// Lifetimes are asserted, not just presence, because at least one of them was a real bug with a
/// call-breaking symptom. See the websocket test below.
/// </para>
/// </remarks>
public class ServiceRegistrationTests
{
    static ServiceDescriptor? Find<T>(IServiceCollection services) =>
        services.FirstOrDefault(d => d.ServiceType == typeof(T));

    [Theory]
    [InlineData(typeof(ILocalMediaStream))]
    [InlineData(typeof(IMediaStreamManager))]
    [InlineData(typeof(IDataManager))]
    [InlineData(typeof(IMediaRecorderManager))]
    [InlineData(typeof(ConnectionParametersViewModel))]
    [InlineData(typeof(CallViewModel))]
    [InlineData(typeof(ChatViewModel))]
    public void Middleware_registers_its_services_as_singletons(Type serviceType)
    {
        var services = new ServiceCollection().AddMiddleware();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == serviceType);

        descriptor.Should().NotBeNull($"{serviceType.Name} is part of the middleware surface");
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void Middleware_brings_the_connection_layer_with_it()
    {
        var services = new ServiceCollection().AddMiddleware();

        Find<IConnectionFactory>(services).Should().NotBeNull(
            "AddMiddleware calls AddConnection, so a consumer never has to know to call both");
    }

    /// <summary>
    /// Both connection kinds are registered against <see cref="IConnection"/>, which is what lets
    /// the factory pick between peer-to-peer signalling and the mediasoup SFU at runtime.
    /// </summary>
    [Fact]
    public void Both_connection_implementations_are_registered()
    {
        var services = new ServiceCollection().AddConnection();

        var connections = services.Where(d => d.ServiceType == typeof(IConnection)).ToArray();

        connections.Should().HaveCount(2, "signalling and mediasoup are both IConnection");
    }

    /// <summary>
    /// A regression test with a symptom attached: these were singletons once. A websocket cannot be
    /// reopened after it closes, so the second call in a session reused a dead socket and the join
    /// simply never arrived anywhere. Transient is not a style preference here.
    /// </summary>
    [Theory]
    [InlineData(typeof(ClientWebSocketSystem))]
    [InlineData(typeof(ClientWebSocketLitePcl))]
    public void Client_websockets_are_transient_because_a_closed_one_cannot_be_reopened(Type socketType)
    {
        var services = new ServiceCollection().AddMediaSoup();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == socketType);

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient,
            "a second call must get a fresh socket, not the dead one from the first");
    }

    [Fact]
    public void The_websocket_factory_itself_is_a_singleton()
    {
        var services = new ServiceCollection().AddMediaSoup();

        Find<ClientWebSocketFactory>(services)!.Lifetime.Should().Be(ServiceLifetime.Singleton,
            "the factory holds no socket state; only what it hands out is per-call");
    }

    /// <summary>
    /// The registrations are plain Add rather than TryAdd, so calling AddMiddleware twice appends a
    /// second set. Recorded rather than asserted against, because nothing calls it twice and making
    /// it idempotent would be a change to production behaviour that nobody has asked for - but it
    /// is worth knowing before someone wires it into two hosts and wonders why
    /// <c>GetServices&lt;IConnection&gt;()</c> returns four.
    /// </summary>
    [Fact]
    public void Registering_the_middleware_twice_appends_rather_than_replacing()
    {
        var once = new ServiceCollection().AddMiddleware().Count;
        var twice = new ServiceCollection().AddMiddleware().AddMiddleware().Count;

        twice.Should().BeGreaterThan(once, "these are Add, not TryAdd");
    }

    /// <summary>
    /// The one deliberate exception to the Add-not-TryAdd rule above.
    /// </summary>
    /// <remarks>
    /// The signalling URL provider is TryAdd so that an app supplying its own is not quietly
    /// overridden by the configuration-backed default. Plain Add would make it depend on the order
    /// the two registrations happen in - register yours before <c>AddSignaling()</c> and the
    /// default, added afterwards, would win on resolution. That is the sort of rule nobody should
    /// have to know. Added for issue #9.
    /// </remarks>
    [Fact]
    public void The_signalling_url_provider_is_registered_once_and_yields_to_an_apps_own()
    {
        var services = new ServiceCollection().AddMiddleware().AddMiddleware();

        services.Count(d => d.ServiceType == typeof(ISignalingServerUrlProvider))
            .Should().Be(1, "TryAdd, so a provider the app registered survives");
    }

    static IServiceProvider ContainerWith(params (string Key, string Value)[] settings) =>
        new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(settings.Select(s =>
                    new KeyValuePair<string, string>(s.Key, s.Value)))
                .Build())
            .AddMiddleware()
            .BuildServiceProvider();

    [Fact]
    public void The_default_provider_reads_the_address_from_configuration()
    {
        var container = ContainerWith(("SignalingServer:BaseUrl", "https://signalling.example.com"));

        container.GetRequiredService<ISignalingServerUrlProvider>().BaseUrl
            .Should().Be("https://signalling.example.com", "that is where it has always come from");
    }

    [Fact]
    public void The_default_provider_says_nothing_rather_than_guessing_when_unconfigured()
    {
        // Null is the "not known yet" the interface documents, and the stub turns it into a message
        // naming both ways of supplying one. It must not become an empty string or a default host.
        var container = ContainerWith();

        container.GetRequiredService<ISignalingServerUrlProvider>().BaseUrl.Should().BeNull();
    }

    [Fact]
    public void An_app_can_supply_its_own_provider_before_or_after_the_defaults()
    {
        // Both orders, because getting this wrong is silent: the app's provider is registered and
        // simply never used.
        var before = new ServiceCollection()
            .AddSingleton<ISignalingServerUrlProvider, FixedUrlProvider>()
            .AddMiddleware()
            .BuildServiceProvider();

        var after = new ServiceCollection()
            .AddMiddleware()
            .AddSingleton<ISignalingServerUrlProvider, FixedUrlProvider>()
            .BuildServiceProvider();

        before.GetRequiredService<ISignalingServerUrlProvider>().Should().BeOfType<FixedUrlProvider>();
        after.GetRequiredService<ISignalingServerUrlProvider>().Should().BeOfType<FixedUrlProvider>();
    }

    sealed class FixedUrlProvider : ISignalingServerUrlProvider
    {
        public string BaseUrl => "https://chosen-at-runtime.example.com";
    }
}
