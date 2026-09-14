using Utilme.SdpTransform;
using UtilmeSdpTransform;
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.Client.Sdp;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// <see cref="CommonUtils"/> reads an SDP offer into the capability model the rest of the
/// mediasoup client works in. It is the boundary between the wire format and everything else,
/// so a fault here surfaces as a codec that mysteriously is not offered.
/// </summary>
/// <remarks>
/// The fixtures are real SDP text run through the same parser production uses - the
/// <c>ToSdp()</c> extension from Utilme.SdpTransform - rather than hand-built object graphs, so
/// these tests exercise the parse as well as the extraction.
/// </remarks>
public class CommonUtilsTests
{
    /// <summary>
    /// A trimmed but structurally real Chrome offer: one audio section and one video section, with
    /// the fmtp, rtcp-fb and extmap attributes the extraction reads.
    /// </summary>
    const string Offer = """
        v=0
        o=- 4611731400430051336 2 IN IP4 127.0.0.1
        s=-
        t=0 0
        a=group:BUNDLE 0 1
        a=msid-semantic: WMS stream
        m=audio 9 UDP/TLS/RTP/SAVPF 111 103
        c=IN IP4 0.0.0.0
        a=rtcp:9 IN IP4 0.0.0.0
        a=ice-ufrag:4ZcD
        a=ice-pwd:2/1muCWoOi3uLifh0NuRHlBn
        a=fingerprint:sha-256 75:74:5A:A6:A4:E5:52:F4:A7:67:4C:01:C7:EE:91:3F:21:3D:A2:E3:53:7B:6F:30:86:F2:30:AA:65:FB:04:24
        a=setup:actpass
        a=mid:0
        a=extmap:1 urn:ietf:params:rtp-hdrext:ssrc-audio-level
        a=sendrecv
        a=rtcp-mux
        a=rtpmap:111 opus/48000/2
        a=rtcp-fb:111 transport-cc
        a=fmtp:111 minptime=10;useinbandfec=1
        a=rtpmap:103 ISAC/16000
        m=video 9 UDP/TLS/RTP/SAVPF 96 97 102
        c=IN IP4 0.0.0.0
        a=rtcp:9 IN IP4 0.0.0.0
        a=ice-ufrag:4ZcD
        a=ice-pwd:2/1muCWoOi3uLifh0NuRHlBn
        a=fingerprint:sha-256 75:74:5A:A6:A4:E5:52:F4:A7:67:4C:01:C7:EE:91:3F:21:3D:A2:E3:53:7B:6F:30:86:F2:30:AA:65:FB:04:24
        a=setup:actpass
        a=mid:1
        a=extmap:2 urn:ietf:params:rtp-hdrext:toffset
        a=sendrecv
        a=rtcp-mux
        a=rtpmap:96 VP8/90000
        a=rtcp-fb:96 goog-remb
        a=rtcp-fb:96 nack
        a=rtcp-fb:96 nack pli
        a=rtpmap:97 rtx/90000
        a=fmtp:97 apt=96
        a=rtpmap:102 H264/90000
        a=fmtp:102 level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=42e01f

        """;

    static Sdp Parse(string text) => text.ReplaceLineEndings("\r\n").ToSdp();

    [Fact]
    public void Reads_every_codec_from_both_media_sections()
    {
        var caps = CommonUtils.ExtractRtpCapabilities(Parse(Offer));

        caps.Codecs.Select(c => c.MimeType).Should()
            .BeEquivalentTo(["audio/opus", "audio/ISAC", "video/VP8", "video/rtx", "video/H264"]);
    }

    [Fact]
    public void Reads_the_payload_type_clock_rate_and_channels()
    {
        var caps = CommonUtils.ExtractRtpCapabilities(Parse(Offer));

        var opus = caps.Codecs.Single(c => c.MimeType == "audio/opus");
        opus.PreferredPayloadType.Should().Be(111);
        opus.ClockRate.Should().Be(48000);
        opus.Channels.Should().Be(2);

        var vp8 = caps.Codecs.Single(c => c.MimeType == "video/VP8");
        vp8.PreferredPayloadType.Should().Be(96);
        vp8.ClockRate.Should().Be(90000);
    }

    [Fact]
    public void Derives_kind_from_the_media_section_the_codec_came_from()
    {
        var caps = CommonUtils.ExtractRtpCapabilities(Parse(Offer));

        caps.Codecs.Single(c => c.MimeType == "audio/opus").Kind.Should().Be(MediaKind.Audio);
        caps.Codecs.Single(c => c.MimeType == "video/VP8").Kind.Should().Be(MediaKind.Video);
    }

    /// <summary>
    /// fmtp lines become the codec's parameters, which is how H.264 profile negotiation gets its
    /// input - see <see cref="H264Tests"/>.
    /// </summary>
    [Fact]
    public void Attaches_fmtp_parameters_to_the_codec_they_name()
    {
        var caps = CommonUtils.ExtractRtpCapabilities(Parse(Offer));

        var h264 = caps.Codecs.Single(c => c.MimeType == "video/H264");
        h264.Parameters.Should().ContainKey("profile-level-id");
        h264.Parameters["profile-level-id"].ToString().Should().Be("42e01f");

        var rtx = caps.Codecs.Single(c => c.MimeType == "video/rtx");
        rtx.Parameters["apt"].ToString().Should().Be("96");
    }

    [Fact]
    public void Collects_every_rtcp_feedback_line_for_a_codec()
    {
        var caps = CommonUtils.ExtractRtpCapabilities(Parse(Offer));

        var vp8 = caps.Codecs.Single(c => c.MimeType == "video/VP8");
        vp8.RtcpFeedback.Should().HaveCount(3);
        vp8.RtcpFeedback.Select(fb => fb.Type).Should().Contain(["goog-remb", "nack"]);
        vp8.RtcpFeedback.Should().Contain(fb => fb.Type == "nack" && fb.Parameter == "pli");
    }

    [Fact]
    public void Reads_the_header_extensions_of_both_kinds()
    {
        var caps = CommonUtils.ExtractRtpCapabilities(Parse(Offer));

        caps.HeaderExtensions.Should().HaveCount(2);
        caps.HeaderExtensions.Should().Contain(e =>
            e.Kind == MediaKind.Audio && e.PreferredId == 1);
        caps.HeaderExtensions.Should().Contain(e =>
            e.Kind == MediaKind.Video && e.PreferredId == 2);
    }

    [Fact]
    public void Reads_the_dtls_fingerprint_and_role()
    {
        var dtls = CommonUtils.ExtractDtlsParameters(Parse(Offer));

        dtls.Role.Should().Be(DtlsRole.Auto, "setup:actpass means either role");
        dtls.Fingerprints.Should().ContainSingle();
        dtls.Fingerprints[0].Algorithm.Should().Be("sha-256");
        dtls.Fingerprints[0].Value.Should().StartWith("75:74:5A");
    }
}
