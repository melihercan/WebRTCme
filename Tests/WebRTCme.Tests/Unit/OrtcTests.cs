using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.Client;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// <see cref="Ortc"/> is the capability negotiation of the mediasoup-client port: it decides which
/// codecs two ends share, what payload types they agree on, and whether this client can send or
/// receive a given track at all.
/// </summary>
/// <remarks>
/// This is the largest body of borrowed logic in the repository - a C# port of mediasoup-client v3
/// - and therefore the most likely to have drifted from the JavaScript it was ported from. None of
/// it needs a device, a peer or a network: it is functions over plain models.
/// </remarks>
public class OrtcTests
{
    readonly Ortc _ortc = new();

    // Kind is set explicitly rather than left to default. MediaKind.Audio is 0, so an unset Kind
    // silently means audio - and in production ValidateRtpCodecCapability derives it from the mime
    // type before any of this runs, so a video capability reaching Ortc always has it.
    static RtpCodecCapability AudioCapability(string mimeType = "audio/opus", int payloadType = 100) => new()
    {
        MimeType = mimeType,
        Kind = MediaKind.Audio,
        PreferredPayloadType = payloadType,
        ClockRate = 48000,
        Channels = 2,
        Parameters = new Dictionary<string, object>(),
        RtcpFeedback = []
    };

    static RtpCodecCapability VideoCapability(string mimeType = "video/VP8", int payloadType = 101) => new()
    {
        MimeType = mimeType,
        Kind = MediaKind.Video,
        PreferredPayloadType = payloadType,
        ClockRate = 90000,
        Parameters = new Dictionary<string, object>(),
        RtcpFeedback = []
    };

    static RtpCodecParameters Codec(string mimeType, int payloadType, int clockRate = 90000,
        int? channels = null) => new()
    {
        MimeType = mimeType,
        PayloadType = payloadType,
        ClockRate = clockRate,
        Channels = channels,
        Parameters = new Dictionary<string, object>(),
        RtcpFeedback = []
    };

    // ---- validation -------------------------------------------------------------------------

    /// <summary>
    /// The validators fill in optional fields as well as rejecting bad input, so a capability that
    /// arrives half-populated comes out usable. That behaviour is relied on downstream.
    /// </summary>
    [Fact]
    public void Validation_fills_in_the_optional_fields_of_a_capability()
    {
        var caps = new RtpCapabilities();

        _ortc.ValidateRtpCapabilities(caps);

        caps.Codecs.Should().NotBeNull().And.BeEmpty();
        caps.HeaderExtensions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Validation_derives_kind_from_the_mime_type()
    {
        var codec = new RtpCodecCapability
        {
            MimeType = "video/VP8",
            ClockRate = 90000,
            Kind = MediaKind.Audio        // deliberately wrong; the mime type wins
        };

        _ortc.ValidateRtpCodecCapability(codec);

        codec.Kind.Should().Be(MediaKind.Video);
    }

    [Fact]
    public void Validation_defaults_audio_to_one_channel_and_strips_channels_from_video()
    {
        var audio = new RtpCodecCapability { MimeType = "audio/PCMU", ClockRate = 8000 };
        var video = new RtpCodecCapability { MimeType = "video/VP8", ClockRate = 90000, Channels = 2 };

        _ortc.ValidateRtpCodecCapability(audio);
        _ortc.ValidateRtpCodecCapability(video);

        audio.Channels.Should().Be(1);
        video.Channels.Should().BeNull("channels are meaningless for video");
    }

    [Theory]
    [InlineData(null)]              // missing entirely
    [InlineData("opus")]            // no media component
    [InlineData("text/plain")]      // not audio or video
    public void Validation_rejects_a_bad_mime_type(string? mimeType)
    {
        var act = () => _ortc.ValidateRtpCodecCapability(new RtpCodecCapability
        {
            MimeType = mimeType!,
            ClockRate = 90000
        });

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Validation_rejects_parameters_with_no_codecs()
    {
        var act = () => _ortc.ValidateRtpParameters(new RtpParameters());

        act.Should().Throw<Exception>().WithMessage("*codecs*");
    }

    // ---- can send / can receive -------------------------------------------------------------

    [Fact]
    public void Can_send_a_kind_that_the_shared_capabilities_carry()
    {
        var extended = new ExtendedRtpCapabilities
        {
            Codecs =
            [
                new ExtendedRtpCodecCapability { MimeType = "video/VP8", Kind = MediaKind.Video, LocalPayloadType = 101, RemotePayloadType = 101 }
            ]
        };

        _ortc.CanSend(MediaKind.Video, extended).Should().BeTrue();
        _ortc.CanSend(MediaKind.Audio, extended).Should().BeFalse();
    }

    /// <summary>
    /// Receiving is decided by the <em>remote</em> payload type of the first codec, because that is
    /// what will actually arrive on the wire.
    /// </summary>
    [Fact]
    public void Can_receive_when_the_first_codec_s_payload_type_is_known_remotely()
    {
        var extended = new ExtendedRtpCapabilities
        {
            Codecs =
            [
                new ExtendedRtpCodecCapability
                {
                    MimeType = "video/VP8", Kind = MediaKind.Video,
                    LocalPayloadType = 96, RemotePayloadType = 101
                }
            ]
        };

        var receivable = new RtpParameters { Codecs = [Codec("video/VP8", 101)] };
        var notReceivable = new RtpParameters { Codecs = [Codec("video/VP8", 96)] };

        _ortc.CanReceive(receivable, extended).Should().BeTrue();
        _ortc.CanReceive(notReceivable, extended)
             .Should().BeFalse("96 is this end's payload type, not the one that arrives");
    }

    [Fact]
    public void Cannot_receive_parameters_carrying_no_codecs()
    {
        var extended = new ExtendedRtpCapabilities { Codecs = [] };

        _ortc.CanReceive(new RtpParameters { Codecs = [] }, extended).Should().BeFalse();
    }

    // ---- reduce codecs ----------------------------------------------------------------------

    /// <summary>
    /// With no preferred codec the first is taken, and its RTX partner comes with it - RTX is
    /// useless without the codec it retransmits.
    /// </summary>
    [Fact]
    public void Reducing_with_no_preference_takes_the_first_codec_and_its_rtx()
    {
        RtpCodecParameters[] codecs =
        [
            Codec("video/VP8", 101),
            Codec("video/rtx", 102),
            Codec("video/H264", 103)
        ];

        var reduced = _ortc.ReduceCodecs(codecs, null);

        reduced.Should().HaveCount(2);
        reduced[0].MimeType.Should().Be("video/VP8");
        reduced[1].MimeType.Should().Be("video/rtx");
    }

    [Fact]
    public void Reducing_with_no_preference_and_no_rtx_takes_only_the_first_codec()
    {
        RtpCodecParameters[] codecs = [Codec("video/VP8", 101), Codec("video/H264", 103)];

        var reduced = _ortc.ReduceCodecs(codecs, null);

        reduced.Should().ContainSingle().Which.MimeType.Should().Be("video/VP8");
    }

    [Fact]
    public void Reducing_to_a_preferred_codec_picks_that_one_and_its_rtx()
    {
        RtpCodecParameters[] codecs =
        [
            Codec("video/VP8", 101),
            Codec("video/rtx", 102),
            Codec("video/H264", 103),
            Codec("video/rtx", 104)
        ];

        var reduced = _ortc.ReduceCodecs(codecs, VideoCapability("video/H264", 103));

        reduced.Should().HaveCount(2);
        reduced[0].MimeType.Should().Be("video/H264");
        reduced[1].PayloadType.Should().Be(104);
    }

    [Fact]
    public void Reducing_to_a_codec_that_is_not_offered_is_an_error()
    {
        RtpCodecParameters[] codecs = [Codec("video/VP8", 101)];

        var act = () => _ortc.ReduceCodecs(codecs, VideoCapability("video/H264", 103));

        act.Should().Throw<Exception>().WithMessage("*No matching codec*");
    }

    /// <summary>
    /// The preferred codec being last in the list is ordinary - a peer that offers VP8 then H264
    /// and prefers H264 produces exactly this - and there is no RTX entry after it to look at.
    /// </summary>
    [Fact]
    public void Reducing_to_a_preferred_codec_that_is_last_in_the_list()
    {
        RtpCodecParameters[] codecs =
        [
            Codec("video/VP8", 101),
            Codec("video/H264", 103)
        ];

        var reduced = _ortc.ReduceCodecs(codecs, VideoCapability("video/H264", 103));

        reduced.Should().ContainSingle().Which.MimeType.Should().Be("video/H264");
    }

    // ---- extended capabilities --------------------------------------------------------------

    /// <summary>
    /// The core of the negotiation: a codec both ends support survives, and the extended capability
    /// records both payload types, which are free to differ.
    /// </summary>
    [Fact]
    public void Extending_keeps_a_codec_both_ends_support_and_records_both_payload_types()
    {
        var local = new RtpCapabilities
        {
            Codecs = [VideoCapability("video/VP8", 96)],
            HeaderExtensions = []
        };
        var remote = new RtpCapabilities
        {
            Codecs = [VideoCapability("video/VP8", 101)],
            HeaderExtensions = []
        };

        var extended = _ortc.GetExtendedRtpCapabilites(local, remote);

        extended.Codecs.Should().ContainSingle();
        extended.Codecs[0].MimeType.Should().Be("video/VP8");
        extended.Codecs[0].LocalPayloadType.Should().Be(96);
        extended.Codecs[0].RemotePayloadType.Should().Be(101);
    }

    [Fact]
    public void Extending_drops_a_codec_only_one_end_supports()
    {
        var local = new RtpCapabilities
        {
            Codecs = [VideoCapability("video/VP8", 96)],
            HeaderExtensions = []
        };
        var remote = new RtpCapabilities
        {
            Codecs = [VideoCapability("video/H264", 101)],
            HeaderExtensions = []
        };

        var extended = _ortc.GetExtendedRtpCapabilites(local, remote);

        extended.Codecs.Should().BeEmpty();
    }

    /// <summary>
    /// Clock rate and channel count are part of codec identity: opus at 48000/2 and opus at
    /// 48000/1 are not the same codec and must not be matched.
    /// </summary>
    [Fact]
    public void Extending_does_not_match_the_same_mime_type_at_a_different_clock_rate()
    {
        var local = new RtpCapabilities
        {
            Codecs = [new RtpCodecCapability
            {
                MimeType = "audio/opus", PreferredPayloadType = 100, ClockRate = 48000, Channels = 2,
                Parameters = new Dictionary<string, object>(), RtcpFeedback = []
            }],
            HeaderExtensions = []
        };
        var remote = new RtpCapabilities
        {
            Codecs = [new RtpCodecCapability
            {
                MimeType = "audio/opus", PreferredPayloadType = 100, ClockRate = 16000, Channels = 2,
                Parameters = new Dictionary<string, object>(), RtcpFeedback = []
            }],
            HeaderExtensions = []
        };

        var extended = _ortc.GetExtendedRtpCapabilites(local, remote);

        extended.Codecs.Should().BeEmpty();
    }

    [Fact]
    public void Sending_parameters_are_built_from_the_extended_capabilities()
    {
        var local = new RtpCapabilities
        {
            Codecs = [VideoCapability("video/VP8", 96)],
            HeaderExtensions = []
        };
        var remote = new RtpCapabilities
        {
            Codecs = [VideoCapability("video/VP8", 101)],
            HeaderExtensions = []
        };
        var extended = _ortc.GetExtendedRtpCapabilites(local, remote);

        var sending = _ortc.GetSendingRtpParameters(MediaKind.Video, extended);

        sending.Codecs.Should().ContainSingle();
        sending.Codecs[0].MimeType.Should().Be("video/VP8");
        sending.Codecs[0].PayloadType.Should().Be(96, "sending uses this end's payload type");
    }

    /// <summary>
    /// "Remote" sending parameters still carry the <em>local</em> payload type, which reads like a
    /// bug and is not: this end sends under its own payload type either way. What "remote" changes
    /// is the codec <em>parameters</em> - the answer is described with the ones the far end
    /// advertised. Both halves are asserted here because the naming invites exactly the wrong fix.
    /// </summary>
    [Fact]
    public void Remote_sending_parameters_keep_the_local_payload_type_but_the_remote_parameters()
    {
        var localCodec = VideoCapability("video/H264", 96);
        localCodec.Parameters = new Dictionary<string, object> { ["profile-level-id"] = "42e01f" };

        var remoteCodec = VideoCapability("video/H264", 101);
        remoteCodec.Parameters = new Dictionary<string, object> { ["profile-level-id"] = "42e015" };

        var local = new RtpCapabilities { Codecs = [localCodec], HeaderExtensions = [] };
        var remote = new RtpCapabilities { Codecs = [remoteCodec], HeaderExtensions = [] };
        var extended = _ortc.GetExtendedRtpCapabilites(local, remote);

        var sending = _ortc.GetSendingRtpParameters(MediaKind.Video, extended);
        var sendingRemote = _ortc.GetSendingRemoteRtpParameters(MediaKind.Video, extended);

        sendingRemote.Codecs[0].PayloadType.Should()
            .Be(sending.Codecs[0].PayloadType).And.Be(96, "this end sends under its own payload type");
        sendingRemote.Codecs[0].Parameters.Should()
            .NotBeSameAs(sending.Codecs[0].Parameters, "the remote end's parameters describe the answer");
    }

    [Fact]
    public void Receiving_capabilities_are_expressed_in_remote_payload_types()
    {
        var local = new RtpCapabilities
        {
            Codecs = [VideoCapability("video/VP8", 96)],
            HeaderExtensions = []
        };
        var remote = new RtpCapabilities
        {
            Codecs = [VideoCapability("video/VP8", 101)],
            HeaderExtensions = []
        };
        var extended = _ortc.GetExtendedRtpCapabilites(local, remote);

        var receiving = _ortc.GetRecvRtpCapabilities(extended);

        receiving.Codecs.Should().ContainSingle();
        receiving.Codecs[0].PreferredPayloadType.Should().Be(101);
    }
}
