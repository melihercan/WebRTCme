using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Net.Http;
using WebRTCme.Connection.Signaling.Server.TurnServerProxies;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// Which of the five TURN proxies actually answer, and which only look like they do.
/// </summary>
/// <remarks>
/// The signalling server offers a choice of five and three of them are stubs whose
/// <c>GetIceServersAsync</c> throws. They are listed as options in the wiki, in the service
/// registrations and in the <c>TurnServer</c> enum, with nothing at any of those sites to say that
/// picking one gets you an exception on the first client to join a room.
///
/// That mattered for issue #42, where someone deployed the server and could not work out why calls
/// crossed a LAN and not the internet: STUN alone cannot relay, TURN was the answer, and two of the
/// three TURN options that appear to exist do not. Startup now refuses to start when one is
/// configured, and these pin the reason so nobody re-documents them as available.
/// </remarks>
public class TurnServerProxyTests
{
    public static TheoryData<ITurnServerProxy> Stubs =>
    [
        new CoturnProxy(),
        new AppRtcProxy(),
        new TwilioProxy(),
    ];

    [Theory]
    [MemberData(nameof(Stubs))]
    public async Task The_unimplemented_proxies_throw_rather_than_returning_nothing(
        ITurnServerProxy proxy)
    {
        // Throwing is not the complaint - a stub should. The complaint is that it is reachable by
        // configuration and indistinguishable from the two that work until a call fails.
        var asking = async () => await proxy.GetIceServersAsync();

        await asking.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task StunOnly_answers_with_servers_and_none_of_them_relay()
    {
        // The default, and the reason a deployment can look healthy and still fail for anyone
        // behind symmetric NAT: STUN discovers an address, it does not carry media.
        var servers = await new StunOnlyProxy().GetIceServersAsync();

        servers.Should().NotBeEmpty();
        servers.SelectMany(s => s.Urls).Should().OnlyContain(url => url.StartsWith("stun:"),
            "no TURN here, whatever the appsettings say");
    }

    [Fact]
    public void Xirsys_is_the_one_hosted_proxy_that_is_implemented()
    {
        // Constructed rather than called: calling it would reach Xirsys over the network. This
        // pins that it takes an IHttpClientFactory, which is what Startup had never registered -
        // so before #42 this proxy could not be resolved even when selected.
        var proxy = new XirsysProxy(
            Substitute.For<IHttpClientFactory>(),
            new ConfigurationBuilder().Build());

        proxy.Should().BeAssignableTo<ITurnServerProxy>();
    }
}
