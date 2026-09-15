namespace WebRTCme.BlazorTests;

/// <summary>
/// The loopback scenarios, run in the browser.
/// </summary>
/// <remarks>
/// <para>
/// The scenarios themselves are the same ones Windows and the phones run -
/// WebRTCme.DeviceTests.Core - so a Blazor failure here is comparable with an Android failure
/// there. What differs is only how they are reached: the page runs them, this reads the results
/// out of the DOM.
/// </para>
/// <para>
/// The whole set runs on one page load, in the order the runner declares, and each [Fact] asserts
/// on its own scenario's line. That is deliberate: a browser start is seconds and a scenario is
/// milliseconds, so a page per test would spend all its time starting Chromium - and the scenarios
/// are independent of each other by construction anyway.
/// </para>
/// </remarks>
public class LoopbackTests : IAsyncLifetime
{
    BlazorRun _run = null!;

    public async ValueTask InitializeAsync() => _run = await BlazorRun.StartAsync();

    public async ValueTask DisposeAsync() => await _run.DisposeAsync();

    /// <summary>Asserts on one scenario, reporting the way the other tiers do.</summary>
    void Verify(string name)
    {
        _run.Summary.Should().NotBeNull(
            "the page must report a summary line; without one the run did not finish, and a run "
            + "that did not finish is a failure rather than zero failures. Console errors: "
            + string.Join(" / ", _run.ConsoleErrors));

        _run.Scenarios.Should().ContainKey(name);

        var scenario = _run.Scenarios[name];

        if (scenario.Outcome == "SKIPPED") Assert.Skip(scenario.Message);

        scenario.Outcome.Should().Be("PASSED", $"{name} reported: {scenario.Message}");
    }

    /// <summary>
    /// On the other four platforms this proves a native library loaded. Here it proves the page
    /// reached the browser's WebRTC API at all - which means the package's JsInterop.js was served
    /// from _content/WebRTCme/ and the JSInterop layer bound to it.
    /// </summary>
    [Fact]
    public void The_browser_webrtc_api_is_reachable_and_a_peer_connection_can_be_created() =>
        Verify("NativeLibraryLoads");

    [Fact]
    public void An_offer_is_produced_and_is_real_sdp() => Verify("OfferIsRealSdp");

    /// <summary>The whole negotiation, end to end, between two connections in one page.</summary>
    [Fact]
    public void Two_peers_negotiate_and_a_data_channel_carries_a_message() =>
        Verify("TwoPeersNegotiateAndCarryAMessage");

    [Fact]
    public void Media_devices_can_be_enumerated_whatever_else_has_happened() =>
        Verify("DevicesEnumerateWhateverElseHasHappened");

    [Fact]
    public void A_completed_call_does_not_change_what_devices_exist() =>
        Verify("ACompletedCallDoesNotChangeWhatDevicesExist");
}

/// <summary>
/// The device-enumeration check again, in a page where no call has happened.
/// </summary>
/// <remarks>
/// Its own page load, and that is not an optimisation to undo. The scenario takes a baseline before
/// the call it is checking, so in a page where a call has already run the baseline is the state
/// after a call and "unchanged" holds trivially. Written without that guard on Windows it passed
/// against the very build it was meant to catch. Every tier re-runs it in isolation for this
/// reason; here isolation is a fresh page rather than a fresh process.
/// </remarks>
public class IsolatedEnumerationTests
{
    [Fact]
    public async Task A_completed_call_does_not_change_what_devices_exist_in_a_fresh_page()
    {
        await using var run = await BlazorRun.StartAsync("ACompletedCallDoesNotChangeWhatDevicesExist");

        run.Summary.Should().NotBeNull(
            "the page must report a summary line. Console errors: " + string.Join(" / ", run.ConsoleErrors));
        run.Summary.Should().Contain("total:1", "the filter should have selected exactly one scenario");

        run.Scenarios.Should().ContainKey("ACompletedCallDoesNotChangeWhatDevicesExist");
        run.Scenarios["ACompletedCallDoesNotChangeWhatDevicesExist"].Outcome.Should().Be(
            "PASSED", run.Scenarios["ACompletedCallDoesNotChangeWhatDevicesExist"].Message);
    }
}
