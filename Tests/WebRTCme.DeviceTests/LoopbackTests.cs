using WebRTCme.DeviceTests.Core;

namespace WebRTCme.DeviceTests;

/// <summary>
/// The loopback scenarios, run on Windows through xUnit.
/// </summary>
/// <remarks>
/// <para>
/// The scenarios themselves live in <see cref="LoopbackScenarios"/>, shared with the device runner
/// app that carries them onto Android, iOS and Mac Catalyst. This class is only the xUnit face of
/// them: one [Fact] per scenario, turning a <see cref="ScenarioResult"/> into a pass, a failure or a
/// skip.
/// </para>
/// <para>
/// Shared rather than duplicated so that a platform cannot quietly end up testing something
/// different from the others. The cost is that a failure message here is a string rather than an
/// assertion diff - worth it, because the alternative is five copies of a negotiation that drift
/// apart one fix at a time.
/// </para>
/// <para>
/// Why Windows gets xUnit and the others do not: Mac Catalyst, iOS and Android build an .app or an
/// .apk rather than an executable, and xUnit v3 refuses to build a test project without an app host
/// - of which there is none for maccatalyst-x64 or for a phone. See the csproj.
/// </para>
/// </remarks>
public class LoopbackTests
{
    /// <summary>Runs one scenario and reports it the way xUnit expects.</summary>
    static async Task Verify(Func<Task<ScenarioResult>> scenario)
    {
        var result = await scenario();

        switch (result.Outcome)
        {
            case ScenarioOutcome.Passed:
                return;
            case ScenarioOutcome.Skipped:
                Assert.Skip(result.Message);
                return;
            default:
                Assert.Fail($"{result.Name} failed after {result.Duration.TotalMilliseconds:F0}ms: {result.Message}");
                return;
        }
    }

    /// <summary>
    /// The cheapest possible proof that the native half is present and callable. A package missing
    /// its native payload fails here, before anything interesting is attempted.
    /// </summary>
    [Fact]
    public Task The_native_library_loads_and_a_peer_connection_can_be_created() =>
        Verify(LoopbackScenarios.NativeLibraryLoads);

    [Fact]
    public Task An_offer_is_produced_and_is_real_sdp() =>
        Verify(LoopbackScenarios.OfferIsRealSdp);

    /// <summary>The whole negotiation, end to end.</summary>
    [Fact]
    public Task Two_peers_negotiate_and_a_data_channel_carries_a_message() =>
        Verify(LoopbackScenarios.TwoPeersNegotiateAndCarryAMessage);

    /// <summary>
    /// An ICE restart on a live connection, which on Windows is a new ABI export.
    /// </summary>
    /// <remarks>
    /// The one tier that can catch an export which is present and does nothing: the scenario
    /// compares the ICE ufrag before and after, and a no-op restart repeats it.
    /// </remarks>
    [Fact]
    public Task An_ice_restart_offers_fresh_credentials_and_the_call_survives() =>
        Verify(LoopbackScenarios.AnIceRestartOffersFreshCredentials);

    /// <summary>
    /// Enumeration answers "what can I use?", and one kind of device being uncountable must not turn
    /// the whole answer into an exception. On Windows a completed call used to leave the audio
    /// device module unable to count inputs, and that threw out of EnumerateDevices carrying the
    /// cameras with it.
    /// </summary>
    [Fact]
    public Task Media_devices_can_be_enumerated_whatever_else_has_happened() =>
        Verify(LoopbackScenarios.DevicesEnumerateWhateverElseHasHappened);

    /// <summary>
    /// A call must not change what devices the machine appears to have.
    /// </summary>
    /// <remarks>
    /// Needs a process in which no call has happened yet, and the scenario checks that rather than
    /// assuming it - written without that guard, it passed against the very build it was meant to
    /// catch. Tests/Test-Device.ps1 re-runs this one on its own for the same reason.
    /// </remarks>
    [Fact]
    public Task A_completed_call_does_not_change_what_devices_exist() =>
        Verify(LoopbackScenarios.ACompletedCallDoesNotChangeWhatDevicesExist);
}
