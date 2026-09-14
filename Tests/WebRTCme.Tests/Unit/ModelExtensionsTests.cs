using System.Text.Json;
using WebRTCme.Connection.MediaSoup;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// <see cref="ModelExtensions.ToStringOrNumber"/> coerces the <see cref="JsonElement"/>s that
/// System.Text.Json leaves in a <c>Dictionary&lt;string, object&gt;</c> into the strings and
/// numbers mediasoup expects.
/// </summary>
/// <remarks>
/// Two of these lock in fixed bugs rather than describing intent, and both took out a live
/// connection. They are marked where they appear.
/// </remarks>
public class ModelExtensionsTests
{
    static Dictionary<string, object> Parse(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

    [Fact]
    public void Converts_a_json_string_to_a_string()
    {
        var converted = Parse("""{"profile-level-id":"42e01f"}""").ToStringOrNumber();

        converted["profile-level-id"].Should().BeOfType<string>().And.Be("42e01f");
    }

    /// <summary>
    /// The regression that took out getRouterRtpCapabilities. Both arms of the conversion's
    /// conditional unify to <see cref="double"/> unless the int arm is cast to object, so every
    /// integer was boxed as a double - and unboxing is exact, so a consumer doing <c>(int)value</c>
    /// threw on a value as ordinary as <c>"apt": 101</c>.
    /// </summary>
    [Fact]
    public void Boxes_a_whole_number_as_int_not_double()
    {
        var converted = Parse("""{"apt":101}""").ToStringOrNumber();

        converted["apt"].Should().BeOfType<int>().And.Be(101);
        ((int)converted["apt"]).Should().Be(101, "unboxing is exact, so the boxed type matters");
    }

    [Fact]
    public void Keeps_a_fractional_number_as_double()
    {
        var converted = Parse("""{"maxFramerate":29.97}""").ToStringOrNumber();

        converted["maxFramerate"].Should().BeOfType<double>().And.Be(29.97);
    }

    /// <summary>
    /// A null is legitimate JSON, and the previous in-place version dereferenced it.
    /// </summary>
    [Fact]
    public void Passes_a_json_null_through_as_null()
    {
        var converted = Parse("""{"appData":null}""").ToStringOrNumber();

        converted.Should().ContainKey("appData");
        converted["appData"].Should().BeNull();
    }

    [Fact]
    public void Leaves_a_value_that_was_never_a_json_element_untouched()
    {
        var already = new Dictionary<string, object> { ["level-asymmetry-allowed"] = 1 };

        var converted = already.ToStringOrNumber();

        converted["level-asymmetry-allowed"].Should().BeOfType<int>().And.Be(1);
    }

    /// <summary>
    /// The conversion used to edit the caller's dictionary in place, which made it invisible at the
    /// call site and left no way to tell a converted dictionary from an untouched one.
    /// </summary>
    [Fact]
    public void Does_not_mutate_the_caller_s_dictionary()
    {
        var original = Parse("""{"apt":101}""");

        var converted = original.ToStringOrNumber();

        converted.Should().NotBeSameAs(original);
        original["apt"].Should().BeOfType<JsonElement>("the caller's dictionary is left alone");
    }

    [Fact]
    public void Rejects_a_bool_unless_bools_are_allowed()
    {
        var act = () => Parse("""{"useinbandfec":true}""").ToStringOrNumber();

        act.Should().Throw<NotSupportedException>()
           .WithMessage("*useinbandfec*", "the key is what identifies the unexpected shape");
    }

    [Fact]
    public void Accepts_a_bool_when_bools_are_allowed()
    {
        var converted = Parse("""{"useinbandfec":true}""").ToStringOrNumberOrBool();

        converted["useinbandfec"].Should().BeOfType<bool>().And.Be(true);
    }

    [Fact]
    public void Rejects_a_shape_this_client_does_not_model()
    {
        var act = () => Parse("""{"nested":{"a":1}}""").ToStringOrNumber();

        act.Should().Throw<NotSupportedException>().WithMessage("*nested*");
    }

    [Fact]
    public void Returns_null_for_a_null_dictionary()
    {
        ((Dictionary<string, object>?)null!).ToStringOrNumber().Should().BeNull();
    }
}
