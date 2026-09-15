namespace WebRTCme.DeviceTests.Core;

/// <summary>How one scenario ended.</summary>
public enum ScenarioOutcome
{
    /// <summary>The scenario ran and its assertions held.</summary>
    Passed,

    /// <summary>The scenario ran and something was wrong. <see cref="ScenarioResult.Message"/> says what.</summary>
    Failed,

    /// <summary>
    /// The scenario could not run for a reason that is about the machine rather than the package -
    /// no camera attached, say. Never used to paper over a failure.
    /// </summary>
    Skipped,
}

/// <param name="Name">The scenario's name, used verbatim as the test name on Windows.</param>
/// <param name="Outcome">Passed, failed or skipped.</param>
/// <param name="Message">Why it failed or was skipped. Empty when it passed.</param>
/// <param name="Duration">How long it took, which is the first clue when something hangs.</param>
public record ScenarioResult(string Name, ScenarioOutcome Outcome, string Message, TimeSpan Duration)
{
    public static ScenarioResult Pass(string name, TimeSpan duration) =>
        new(name, ScenarioOutcome.Passed, string.Empty, duration);

    public static ScenarioResult Fail(string name, string message, TimeSpan duration) =>
        new(name, ScenarioOutcome.Failed, message, duration);

    public static ScenarioResult Skip(string name, string message, TimeSpan duration) =>
        new(name, ScenarioOutcome.Skipped, message, duration);

    /// <summary>
    /// One line per result, in a shape that survives a device log and is trivial to grep.
    /// The device runner writes these; the harness on the other end reads them.
    /// </summary>
    public string ToLine() =>
        $"WEBRTCME-SCENARIO | {Outcome.ToString().ToUpperInvariant(),-7} | {Duration.TotalMilliseconds,7:F0}ms | {Name}"
        + (Message.Length == 0 ? string.Empty : $" | {Message.ReplaceLineEndings(" ")}");
}
