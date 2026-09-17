using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Services;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// Who a chat message is sent to, and what happens when the answer is "nobody".
/// </summary>
/// <remarks>
/// There are two ways of having somewhere to send and they belong to different call types. A mesh
/// call fills <c>_peers</c>, one data channel per peer. A MediaSoup call fills a single producer
/// channel instead and leaves <c>_peers</c> empty - producers and consumers do not exist in plain
/// signalling usage.
///
/// So "no peers" does not imply "there is a producer channel", and for years the code assumed it
/// did: the else-branch dereferenced a channel that is null by definition on a mesh call. Typing a
/// message before anyone else joined the room took the app down with a NullReferenceException.
/// That was issue #31, reported in 2023 against the MediaSoup chat view.
///
/// Being alone in a room is not an error, which is what makes the first test here the point of the
/// file rather than a defensive extra.
/// </remarks>
public class DataManagerTests
{
    static DataManager Create() =>
        new(Substitute.For<IWebRtcIncomingFileStreamFactory>(),
            NullLogger<DataManager>.Instance);

    static Message AMessage => new() { Text = "hello" };

    [Fact]
    public void SendingWithNobodyToSendToDoesNotThrow()
    {
        var manager = Create();

        var send = () => manager.SendMessage(AMessage);

        send.Should().NotThrow();
    }

    [Fact]
    public void SendingWithNobodyToSendToStillShowsTheMessageLocally()
    {
        // The caller adds the outgoing message to the list before it goes anywhere, so a send that
        // reaches no one must still leave the chat looking right rather than half-updated.
        var manager = Create();

        manager.SendMessage(AMessage);

        manager.DataParametersList.Should().ContainSingle(dp => dp.From == DataFromType.Outgoing);
    }

    [Fact]
    public void AMeshCallSendsToEveryPeer()
    {
        var manager = Create();
        var first = Substitute.For<IRTCDataChannel>();
        var second = Substitute.For<IRTCDataChannel>();

        manager.AddPeer("alice", first, producerDataChannel: null, consumerDataChannel: null);
        manager.AddPeer("bob", second, producerDataChannel: null, consumerDataChannel: null);

        manager.SendMessage(AMessage);

        first.Received(1).Send(Arg.Any<object>());
        second.Received(1).Send(Arg.Any<object>());
    }

    [Fact]
    public void AMediaSoupCallSendsToTheProducerChannel()
    {
        var manager = Create();
        var producer = Substitute.For<IRTCDataChannel>();

        manager.AddPeer("sfu", dataChannel: null, producerDataChannel: producer,
            consumerDataChannel: null);

        manager.SendMessage(AMessage);

        producer.Received(1).Send(Arg.Any<object>());
    }
}
