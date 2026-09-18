using System.Diagnostics;

namespace WebRTCme.DeviceTests.Runner;

/// <summary>
/// Turns libwebrtc's own logging on, so a run records what the native library did rather than what
/// a reconstruction of it does.
/// </summary>
/// <remarks>
/// <para>
/// Written for the Apple crash of September 2026: an <c>EXC_BAD_ACCESS</c> at <c>0x30</c> inside
/// <c>JsepTransportController::MaybeStartGathering</c>, on libwebrtc's signalling thread, with no
/// managed frame anywhere near it. Twelve hypotheses about what causes it were refuted by
/// measurement, and every hand-written probe of the supposed mechanism stayed clean while only the
/// full scenario suite reproduced. At that point the useful move is to stop guessing and let the
/// library say what it did.
/// </para>
/// <para>
/// Off unless <c>WEBRTCME_WEBRTC_LOG</c> is set, and that is not tidiness. The fault is
/// probabilistic and timing-sensitive - it dies about 85% of runs in the configuration that
/// reproduces - so logging on every run would perturb the thing being measured. Set it when
/// diagnosing, leave it unset when measuring a rate.
/// </para>
/// <para>
/// The value picks the severity: <c>verbose</c>, <c>info</c> (the default), <c>warning</c> or
/// <c>error</c>. Verbose is a firehose and slows the process enough to matter here; info carries
/// the peer connection and transport lifecycle, which is what the question is about.
/// </para>
/// <para>
/// Lines are prefixed and go out through the same two channels as everything else, because on
/// Apple platforms stdout and the system log are different pipes and which one a launch mechanism
/// captures varies. The prefix is deliberately not <c>WEBRTCME-SCENARIO</c> or
/// <c>WEBRTCME-SUMMARY</c>: the harness greps for those, and native log lines must not be mistaken
/// for results.
/// </para>
/// </remarks>
internal static partial class NativeLogging
{
    internal const string EnableVariable = "WEBRTCME_WEBRTC_LOG";

    /// <summary>Prefix for every native line. Distinct from the prefixes the harness parses.</summary>
    internal const string Prefix = "WEBRTCME-NATIVE";

    /// <summary>Starts native logging if the environment asks for it. Never throws.</summary>
    internal static void StartIfRequested()
    {
        var requested = Environment.GetEnvironmentVariable(EnableVariable);
        if (string.IsNullOrEmpty(requested) || requested == "0")
            return;

        try
        {
            StartCore(requested, Emit);
            Emit($"logging started at '{requested}'");
        }
        catch (Exception exception)
        {
            // Diagnostics must never be why a run fails.
            Emit($"could not start: {exception.GetType().Name}: {exception.Message}");
        }
    }

    static void Emit(string line)
    {
        var text = $"{Prefix} | {line}";
        Console.WriteLine(text);
        Debug.WriteLine(text);
    }

    /// <summary>
    /// Implemented per platform. Unimplemented ones simply do nothing, which is why this is a
    /// partial method rather than a throw: Android and Windows have their own logging story and
    /// this class exists for the Apple question.
    /// </summary>
    static partial void StartCore(string severity, Action<string> report);
}
