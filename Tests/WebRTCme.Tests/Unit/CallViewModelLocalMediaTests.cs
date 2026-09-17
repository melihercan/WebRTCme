using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WebRTCme.Connection;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Models;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// What a call does when the machine cannot open everything it asked for.
/// </summary>
/// <remarks>
/// Opening the local stream asks for a camera and a microphone together, and a machine that has
/// only one of them is ordinary - a desktop with a headset, a laptop with the camera disabled by
/// policy. That used to throw out of <c>OnPageAppearingAsync</c>, which runs from page-appearing
/// with nobody to catch it, and the app stopped. Reported as issue #30 in 2023, in one line: "if
/// the computer does not have a camera, the program will stop".
///
/// The rule this pins is the one the mid-call recovery in the same class already follows - a call
/// carrying one fewer track beats a call that falls over - plus the part that recovery does not
/// need: when nothing at all can be opened there is a person watching, so say so.
/// </remarks>
public class CallViewModelLocalMediaTests
{
    static readonly ConnectionParameters Parameters = new()
    {
        ConnectionType = ConnectionType.Signaling,
        Name = "tester",
        Room = "room"
    };

    static (CallViewModel ViewModel, ILocalMediaStream Media, IModalPopup Popup,
            IConnectionFactory Connections) Create()
    {
        var media = Substitute.For<ILocalMediaStream>();
        var popup = Substitute.For<IModalPopup>();
        var connections = Substitute.For<IConnectionFactory>();

        var streams = Substitute.For<IMediaStreamManager>();
        streams.MediaStreamParametersList.Returns(new ObservableCollection<MediaStreamParameters>());

        var viewModel = new CallViewModel(
            Substitute.For<INavigation>(),
            media,
            streams,
            Substitute.For<IMediaRecorderManager>(),
            popup,
            Substitute.For<IRunOnUiThread>(),
            NullLogger<CallViewModel>.Instance,
            connections);

        return (viewModel, media, popup, connections);
    }

    [Fact]
    public async Task NoCaptureDevicesAtAllDoesNotTakeTheAppDown()
    {
        var (viewModel, media, _, _) = Create();
        media.GetCameraMediaStreamAsync(Arg.Any<CameraType>(), Arg.Any<MediaStreamConstraints>())
            .ThrowsAsync(new InvalidOperationException("no capture devices"));

        var appearing = async () => await viewModel.OnPageAppearingAsync(Parameters);

        await appearing.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NoCaptureDevicesAtAllSaysSoRatherThanFailingSilently()
    {
        var (viewModel, media, popup, _) = Create();
        media.GetCameraMediaStreamAsync(Arg.Any<CameraType>(), Arg.Any<MediaStreamConstraints>())
            .ThrowsAsync(new InvalidOperationException("no capture devices"));

        await viewModel.OnPageAppearingAsync(Parameters);

        await popup.Received(1).GenericPopupAsync(Arg.Any<GenericPopupIn>());
    }

    [Fact]
    public async Task NoCaptureDevicesAtAllDoesNotStartACall()
    {
        // Joining a room with nothing to send would put an empty tile in front of everyone else
        // and leave this end wondering why it is silent.
        var (viewModel, media, _, connections) = Create();
        media.GetCameraMediaStreamAsync(Arg.Any<CameraType>(), Arg.Any<MediaStreamConstraints>())
            .ThrowsAsync(new InvalidOperationException("no capture devices"));

        await viewModel.OnPageAppearingAsync(Parameters);

        connections.DidNotReceive().SelectConnection(Arg.Any<ConnectionType>());
    }

    [Fact]
    public async Task BothFirst_ThenTheMicrophone_ThenTheCamera()
    {
        // The order matters: a microphone-only call is useful and a camera-only one is the odd
        // case, so the microphone is tried first. Asserted through the all-fail path because that
        // is the one run that makes every attempt.
        var (viewModel, media, _, _) = Create();
        var asked = new List<MediaStreamConstraints>();
        media.GetCameraMediaStreamAsync(Arg.Any<CameraType>(), Arg.Any<MediaStreamConstraints>())
            .ThrowsAsync(call =>
            {
                asked.Add(call.Arg<MediaStreamConstraints>());
                return new InvalidOperationException("no capture devices");
            });

        await viewModel.OnPageAppearingAsync(Parameters);

        asked.Should().HaveCount(3);
        asked[0].Should().BeNull("the first attempt asks for the pair, as before");
        asked[1]!.Audio.Should().NotBeNull("the second attempt is the microphone alone");
        asked[1]!.Video.Should().BeNull();
        asked[2]!.Video.Should().NotBeNull("the third attempt is the camera alone");
        asked[2]!.Audio.Should().BeNull();
    }
}
