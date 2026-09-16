namespace WebRTCme.DeviceTests.Core;

/// <summary>
/// Progress messages that survive a process that never finishes.
/// </summary>
/// <remarks>
/// <para>
/// Standard error, deliberately, and flushed on every line. xUnit captures <c>Console.Out</c> for
/// the duration of a test and prints it when the test ends - so a test that never ends prints
/// nothing at all, and every diagnostic written the obvious way dies with the process. That is
/// exactly the state a hosted Windows runner reached: "Running the scenarios..." and then not one
/// further line, no xUnit banner, nothing, until the job timed out.
/// </para>
/// <para>
/// stderr is not captured, and the tier 3 harness redirects it to a file and prints that file even
/// when it has to kill the process. So these lines are readable precisely when everything else is
/// not.
/// </para>
/// <para>
/// They are cheap and unconditional. Five scenarios produce a dozen lines, every harness already
/// filters on the <c>WEBRTCME-</c> prefix, and a trace that has to be switched on is a trace nobody
/// has switched on when they need it.
/// </para>
/// </remarks>
public static class ScenarioTrace
{
    /// <summary>Prefix every line carries, so a harness can pick these out of a device log.</summary>
    public const string Prefix = "WEBRTCME-TRACE";

    /// <summary>Writes one progress line and flushes it.</summary>
    public static void Write(string message)
    {
        try
        {
            Console.Error.WriteLine($"{Prefix} | {message}");
            Console.Error.Flush();
        }
        catch
        {
            // A platform that cannot write to stderr must not fail a run because of it. Android
            // routes both streams to logcat, iOS to the system log, and a sandboxed process may
            // have neither - none of which is a reason to lose the test.
        }
    }
}
