using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WebRTCme.Connection;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Models;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// What turning your own camera off does to your own tile.
/// </summary>
/// <remarks>
/// <para>
/// Muting disables the outgoing track, which is the whole of the contract for everybody else:
/// their tile stops receiving frames and covers itself. It is not the whole of it for the machine
/// that pressed the button. On Windows the local tile renders that same disabled track, so it
/// goes dark for free; on Apple the local tile is an <c>RTCCameraPreviewView</c> fed straight from
/// the capture session, which is still running - so the one person who had just turned their
/// camera off was the only person in the call who could still see it. Measured on Mac Catalyst on
/// 2026-09-22, against Windows, in both directions.
/// </para>
/// <para>
/// So the view model has to say so out loud, and these pin that it does. Two things have to hold
/// for the platform to hear it: the tile's flag must follow the button, and it must be raised on
/// an object that announces the change - <c>MediaStreamParameters.VideoMuted</c> was a plain
/// auto-property, so setting it was silent and no renderer ever repainted.
/// </para>
/// </remarks>
public class CallViewModelMutePreviewTests
{
    static readonly ConnectionParameters Parameters = new()
    {
        ConnectionType = ConnectionType.Signaling,
        Name = "tester",
        Room = "room"
    };

    sealed class Harness
    {
        public CallViewModel ViewModel { get; init; }
        public IConnection Connection { get; init; }
        public ObservableCollection<MediaStreamParameters> Tiles { get; init; }

        public MediaStreamParameters LocalTile =>
            Tiles.Single(tile => tile.IsLocal);
    }

    static Harness Create()
    {
        var stream = Substitute.For<IMediaStream>();
        stream.GetTracks().Returns([]);
        stream.GetVideoTracks().Returns([]);
        stream.GetAudioTracks().Returns([]);

        var media = Substitute.For<ILocalMediaStream>();
        media.GetCameraMediaStreamAsync(Arg.Any<CameraType>(), Arg.Any<MediaStreamConstraints>())
            .Returns(stream);

        var connection = Substitute.For<IConnection>();
        connection.ConnectionRequest(Arg.Any<UserContext>())
            .Returns(Observable.Never<PeerResponse>());

        var connections = Substitute.For<IConnectionFactory>();
        connections.SelectConnection(Arg.Any<ConnectionType>()).Returns(connection);

        var tiles = new ObservableCollection<MediaStreamParameters>();
        var streams = Substitute.For<IMediaStreamManager>();
        streams.MediaStreamParametersList.Returns(tiles);
        streams.When(manager => manager.Add(Arg.Any<MediaStreamParameters>()))
            .Do(call => tiles.Add(call.Arg<MediaStreamParameters>()));

        // Runs what it is handed, rather than swallowing it. A substitute's default Invoke does
        // nothing, which would make every one of these pass for the wrong reason.
        var ui = Substitute.For<IRunOnUiThread>();
        ui.When(thread => thread.Invoke(Arg.Any<Action>()))
            .Do(call => call.Arg<Action>().Invoke());

        var viewModel = new CallViewModel(
            Substitute.For<INavigation>(),
            media,
            streams,
            Substitute.For<IMediaRecorderManager>(),
            Substitute.For<IModalPopup>(),
            ui,
            NullLogger<CallViewModel>.Instance,
            connections);

        return new Harness { ViewModel = viewModel, Connection = connection, Tiles = tiles };
    }

    static async Task<Harness> JoinedAsync()
    {
        var harness = Create();
        await harness.ViewModel.OnPageAppearingAsync(Parameters);
        return harness;
    }

    [Fact]
    public async Task MutingTheCameraCoversTheLocalTile()
    {
        var harness = await JoinedAsync();

        await harness.ViewModel.OnToggleCameraAsync();

        harness.ViewModel.IsCameraMuted.Should().BeTrue();
        harness.LocalTile.VideoMuted.Should().BeTrue(
            "the tile is what the platform reads, and Apple's preview will not go dark by itself");
    }

    [Fact]
    public async Task UnmutingTheCameraUncoversIt()
    {
        var harness = await JoinedAsync();

        await harness.ViewModel.OnToggleCameraAsync();
        await harness.ViewModel.OnToggleCameraAsync();

        harness.ViewModel.IsCameraMuted.Should().BeFalse();
        harness.LocalTile.VideoMuted.Should().BeFalse();
    }

    [Fact]
    public async Task TheLocalTileAnnouncesTheChange()
    {
        // The flag being right is not enough: a renderer only repaints when it is told, and a
        // plain auto-property tells nobody.
        var harness = await JoinedAsync();

        var announced = new List<string>();
        ((INotifyPropertyChanged)harness.LocalTile).PropertyChanged +=
            (_, args) => announced.Add(args.PropertyName);

        await harness.ViewModel.OnToggleCameraAsync();

        announced.Should().Contain(nameof(MediaStreamParameters.VideoMuted));
    }

    [Fact]
    public async Task AMuteThatFailedLeavesTheTileAlone()
    {
        // A preview that covers itself when the track is still live is a worse lie than one that
        // does not cover itself at all: it says "they cannot see you" when they can.
        var harness = await JoinedAsync();
        harness.Connection
            .SetOutgoingMediaEnabledAsync(Arg.Any<MediaStreamTrackKind>(), Arg.Any<bool>())
            .ThrowsAsync(new InvalidOperationException("there is no call to mute"));

        await harness.ViewModel.OnToggleCameraAsync();

        harness.ViewModel.IsCameraMuted.Should().BeFalse();
        harness.LocalTile.VideoMuted.Should().BeFalse();
    }

    [Fact]
    public async Task MutingTheMicrophoneLeavesThePictureAlone()
    {
        var harness = await JoinedAsync();

        await harness.ViewModel.OnToggleMicrophoneAsync();

        harness.ViewModel.IsMicrophoneMuted.Should().BeTrue();
        harness.LocalTile.VideoMuted.Should().BeFalse();
    }
}
