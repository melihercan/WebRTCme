using WebRTCme.Connection.MediaSoup.Client;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// <see cref="ScalabilityModes.Parse"/> reads an SVC scalability mode - "L3T3", "S2T1" - into its
/// spatial and temporal layer counts, falling back to one layer of each for anything it does not
/// recognise.
/// </summary>
/// <remarks>
/// The fallback is the interesting half. It is what decides how many simulcast layers get
/// configured, and KnownGaps.md records a ladder that had to be matched to the camera before
/// simulcast worked at all - so a mode silently parsing as 1x1 is a real failure mode, not a
/// theoretical one.
/// </remarks>
public class ScalabilityModesTests
{
    [Theory]
    [InlineData("L1T1", 1, 1)]
    [InlineData("L1T3", 1, 3)]
    [InlineData("L3T3", 3, 3)]
    [InlineData("S2T1", 2, 1)]
    [InlineData("S3T3", 3, 3)]
    public void Reads_the_spatial_and_temporal_layer_counts(string mode, int spatial, int temporal)
    {
        var parsed = ScalabilityModes.Parse(mode);

        parsed.SpatialLayers.Should().Be(spatial);
        parsed.TemporalLayers.Should().Be(temporal);
    }

    /// <summary>Two digits per layer count are allowed by the regex.</summary>
    [Theory]
    [InlineData("L10T2", 10, 2)]
    [InlineData("L2T10", 2, 10)]
    public void Reads_two_digit_layer_counts(string mode, int spatial, int temporal)
    {
        var parsed = ScalabilityModes.Parse(mode);

        parsed.SpatialLayers.Should().Be(spatial);
        parsed.TemporalLayers.Should().Be(temporal);
    }

    /// <summary>
    /// Anything unrecognised means one spatial and one temporal layer - that is, no SVC at all.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("nonsense")]
    [InlineData("L0T1")]      // zero is excluded by the regex: [1-9] leads each count
    [InlineData("T3L1")]      // the other way round
    [InlineData("X3T3")]      // neither L nor S
    [InlineData("3T3")]       // no leading letter
    public void Falls_back_to_a_single_layer_for_anything_unrecognised(string? mode)
    {
        var parsed = ScalabilityModes.Parse(mode!);

        parsed.SpatialLayers.Should().Be(1);
        parsed.TemporalLayers.Should().Be(1);
    }

    /// <summary>
    /// The regex is anchored at the start only, so a trailing suffix - "L3T3_KEY" is a real mode
    /// in the SVC spec - still reads its layer counts rather than falling back.
    /// </summary>
    [Fact]
    public void Reads_a_mode_carrying_a_suffix()
    {
        var parsed = ScalabilityModes.Parse("L3T3_KEY");

        parsed.SpatialLayers.Should().Be(3);
        parsed.TemporalLayers.Should().Be(3);
    }

    [Fact]
    public void Defaults_to_a_single_layer_when_called_with_no_argument()
    {
        var parsed = ScalabilityModes.Parse();

        parsed.SpatialLayers.Should().Be(1);
        parsed.TemporalLayers.Should().Be(1);
    }
}
