using WebRTCme.Connection.MediaSoup.Codecs;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// H.264 profile-level-id handling, ported from libwebrtc's own implementation by way of
/// mediasoup-client. These are the rules that decide whether two peers can talk H.264 to each
/// other at all, and what level the answer offers.
/// </summary>
/// <remarks>
/// The profile constants are private to <c>H264</c>, so profiles are compared to each other rather
/// than to a named value, and levels are asserted through the string form. That is also the form
/// that actually travels in SDP, so a round-trip assertion is closer to what matters than
/// inspecting the parsed struct would be.
/// </remarks>
public class H264Tests
{
    static Dictionary<string, object> Params(string? profileLevelId = null, int? levelAsymmetryAllowed = null)
    {
        var p = new Dictionary<string, object>();
        if (profileLevelId is not null) p["profile-level-id"] = profileLevelId;
        if (levelAsymmetryAllowed is not null) p["level-asymmetry-allowed"] = levelAsymmetryAllowed.Value;
        return p;
    }

    /// <summary>
    /// Every profile's canonical string survives parse and re-serialisation unchanged. "42e01f" is
    /// constrained baseline 3.1, the one nearly every browser offers.
    /// </summary>
    [Theory]
    [InlineData("42e01f")]   // constrained baseline, level 3.1
    [InlineData("4d001f")]   // main, level 3.1
    [InlineData("64001f")]   // high, level 3.1
    [InlineData("640c1f")]   // constrained high, level 3.1
    [InlineData("42e015")]   // constrained baseline, level 2.1
    public void Round_trips_a_profile_level_id(string profileLevelId)
    {
        var parsed = H264.ParseProfileLevelId(profileLevelId);

        parsed.Should().NotBeNull();
        H264.ProfileLevelIdToString(parsed).Should().Be(profileLevelId);
    }

    /// <summary>
    /// Level 1b is the awkward one: it is not a level number but constraint set 3 riding on level
    /// 1.1, and it serialises differently per profile. Both directions are handled specially in
    /// the code, so both are worth pinning.
    /// </summary>
    [Theory]
    [InlineData("42f00b")]   // constrained baseline, level 1b
    [InlineData("42100b")]   // baseline, level 1b
    [InlineData("4d100b")]   // main, level 1b
    public void Round_trips_the_level_1b_special_case(string profileLevelId)
    {
        var parsed = H264.ParseProfileLevelId(profileLevelId);

        parsed.Should().NotBeNull();
        H264.ProfileLevelIdToString(parsed).Should().Be(profileLevelId);
    }

    [Theory]
    [InlineData("")]            // not six characters
    [InlineData("42e01")]       // five
    [InlineData("42e01f0")]     // seven
    [InlineData("000000")]      // zero is not a profile
    [InlineData("42e0ff")]      // 0xff is not a level
    [InlineData("ffe01f")]      // no profile pattern matches profile_idc 0xff
    public void Rejects_a_malformed_profile_level_id(string profileLevelId)
    {
        H264.ParseProfileLevelId(profileLevelId).Should().BeNull();
    }

    [Fact]
    public void Treats_a_missing_profile_level_id_as_constrained_baseline_3_1()
    {
        var parsed = H264.ParseSdpProfileLevelId(Params());

        parsed.Should().NotBeNull();
        H264.ProfileLevelIdToString(parsed).Should().Be("42e01f");
    }

    [Fact]
    public void Same_profile_at_different_levels_is_the_same_profile()
    {
        H264.IsSameProfile(Params("42e01f"), Params("42e015")).Should().BeTrue();
    }

    [Fact]
    public void Different_profiles_are_not_the_same_profile()
    {
        H264.IsSameProfile(Params("42e01f"), Params("64001f")).Should().BeFalse();
    }

    /// <summary>
    /// Both sides default to constrained baseline when neither states a profile, so they match.
    /// </summary>
    [Fact]
    public void Two_parameter_sets_with_no_profile_match_by_default()
    {
        H264.IsSameProfile(Params(), Params()).Should().BeTrue();
    }

    /// <summary>
    /// Without level asymmetry the answer must offer the lower of the two levels, so neither peer
    /// is asked for more than it said it could decode.
    /// </summary>
    [Fact]
    public void Answers_with_the_lower_level_when_asymmetry_is_not_allowed()
    {
        var answer = H264.GenerateProfileLevelIdForAnswer(
            localSupportedParams: Params("42e01f"),   // level 3.1
            remoteOfferedParams: Params("42e015"));   // level 2.1

        answer.Should().Be("42e015");
    }

    /// <summary>
    /// With asymmetry allowed by both sides the answer keeps the local level, which is the point
    /// of the flag: each direction may run at its own level.
    /// </summary>
    [Fact]
    public void Answers_with_the_local_level_when_both_sides_allow_asymmetry()
    {
        var answer = H264.GenerateProfileLevelIdForAnswer(
            localSupportedParams: Params("42e01f", levelAsymmetryAllowed: 1),
            remoteOfferedParams: Params("42e015", levelAsymmetryAllowed: 1));

        answer.Should().Be("42e01f");
    }

    /// <summary>One side declining asymmetry is enough to fall back to the lower level.</summary>
    [Fact]
    public void One_side_declining_asymmetry_forces_the_lower_level()
    {
        var answer = H264.GenerateProfileLevelIdForAnswer(
            localSupportedParams: Params("42e01f", levelAsymmetryAllowed: 1),
            remoteOfferedParams: Params("42e015", levelAsymmetryAllowed: 0));

        answer.Should().Be("42e015");
    }

    [Fact]
    public void Refuses_to_answer_across_different_profiles()
    {
        var answer = H264.GenerateProfileLevelIdForAnswer(
            localSupportedParams: Params("42e01f"),   // constrained baseline
            remoteOfferedParams: Params("64001f"));   // high

        answer.Should().BeNull();
    }

    [Fact]
    public void Refuses_to_answer_when_neither_side_states_a_profile()
    {
        H264.GenerateProfileLevelIdForAnswer(Params(), Params()).Should().BeNull();
    }

    /// <summary>
    /// Level 1b sorts below level 1, which plain byte comparison gets wrong: 1b is encoded as 0.
    /// The comparison is special-cased, and this is what that case is for.
    /// </summary>
    [Fact]
    public void Orders_level_1b_below_level_1()
    {
        var answer = H264.GenerateProfileLevelIdForAnswer(
            localSupportedParams: Params("42e00a"),   // level 1
            remoteOfferedParams: Params("42f00b"));   // level 1b

        answer.Should().Be("42f00b", "1b is the lower level even though it encodes as 0");
    }
}
