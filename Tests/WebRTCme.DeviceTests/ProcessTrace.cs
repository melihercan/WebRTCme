using System.Runtime.CompilerServices;
using WebRTCme.DeviceTests.Core;

namespace WebRTCme.DeviceTests;

/// <summary>
/// Says the process got this far, before xUnit says anything.
/// </summary>
/// <remarks>
/// <para>
/// A module initializer runs when the assembly is loaded - before the test runner starts, before
/// discovery, before any test. That makes it the one thing that can distinguish two failures which
/// otherwise look identical from outside:
/// </para>
/// <list type="bullet">
///   <item>nothing at all - the process did not start, or could not load this assembly.</item>
///   <item>this line and no xUnit banner - the process started and the runner never did, so the
///         fault is in startup rather than in any test.</item>
///   <item>this line, a banner, and a <c>begin</c> trace with no <c>end</c> - a scenario blocked,
///         and the trace names it.</item>
/// </list>
/// <para>
/// Worth the file because a hosted Windows runner produced the first of those three for six hours
/// and there was no way to tell which it was.
/// </para>
/// </remarks>
internal static class ProcessTrace
{
    [ModuleInitializer]
    internal static void Announce() =>
        ScenarioTrace.Write(
            $"test assembly loaded - {Environment.OSVersion}, {(Environment.Is64BitProcess ? "64" : "32")}-bit, "
            + $"{Environment.ProcessorCount} cpus");
}
