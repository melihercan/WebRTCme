using System.Text;

namespace WebRTCme.DeviceTests.Core;

/// <summary>
/// Runs the scenarios and reports them in a shape a harness on the other end of a device log can
/// read.
/// </summary>
/// <remarks>
/// <para>
/// The device runner app calls this. It writes each result as a single line and then a summary
/// line, because that is the lowest common denominator across the three platforms: adb logcat on
/// Android, devicectl's console on iOS, and stdout on Mac Catalyst all carry lines and nothing
/// richer. A results file is written too where the platform has somewhere to put one, since a log
/// can be truncated by the tooling in front of it.
/// </para>
/// <para>
/// One scenario is run in isolation by default, and that is not an optimisation. The Windows audio
/// device module used to stop counting microphones once a negotiation had reached DTLS, and the
/// test for it takes a baseline first - so in a process where a call has already run, its baseline
/// is the broken one and it passes trivially. The device runner therefore accepts a filter, and the
/// harness invokes it twice: once for everything, once for that scenario alone.
/// </para>
/// </remarks>
public static class ScenarioRunner
{
    /// <summary>Marks the end of the run, so a reader knows the log is complete rather than cut off.</summary>
    public const string SummaryPrefix = "WEBRTCME-SUMMARY";

    /// <summary>Written first, so a reader can tell a started run from an app that died on launch.</summary>
    public const string StartPrefix = "WEBRTCME-START";

    /// <summary>
    /// Runs the scenarios whose names contain <paramref name="filter"/>, or all of them when it is
    /// null or empty.
    /// </summary>
    /// <param name="report">Where each line goes - a logger on Android, Console.WriteLine elsewhere.</param>
    /// <param name="filter">Substring match on the scenario name, case-insensitive.</param>
    /// <returns>True when nothing failed. Skips do not fail a run.</returns>
    public static async Task<bool> RunAsync(Action<string> report, string? filter = null)
    {
        var selected = LoopbackScenarios.All
            .Where(s => string.IsNullOrEmpty(filter)
                        || s.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        report($"{StartPrefix} | {selected.Count} scenario(s)"
               + (string.IsNullOrEmpty(filter) ? string.Empty : $" matching '{filter}'"));

        var results = new List<ScenarioResult>(selected.Count);
        foreach (var scenario in selected)
        {
            var result = await scenario.Run();
            results.Add(result);
            report(result.ToLine());
        }

        var passed = results.Count(r => r.Outcome == ScenarioOutcome.Passed);
        var failed = results.Count(r => r.Outcome == ScenarioOutcome.Failed);
        var skipped = results.Count(r => r.Outcome == ScenarioOutcome.Skipped);

        report($"{SummaryPrefix} | total:{results.Count} passed:{passed} failed:{failed} skipped:{skipped}");
        return failed == 0;
    }

    /// <summary>
    /// The whole run as text, for writing next to the app where the platform allows it. A log can be
    /// truncated or interleaved by the tooling in front of it; a file cannot.
    /// </summary>
    public static async Task<(bool Ok, string Report)> RunToTextAsync(string? filter = null)
    {
        var builder = new StringBuilder();
        var ok = await RunAsync(line => builder.AppendLine(line), filter);
        return (ok, builder.ToString());
    }
}
