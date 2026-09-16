using System.Runtime.CompilerServices;

namespace MinimalXunit;

/// <summary>
/// One assertion that cannot fail, and a line that proves the process got here.
/// </summary>
/// <remarks>
/// The test itself is beside the point. What is being measured is whether xUnit starts at all on
/// this target framework and this machine - so the module initializer below matters more than the
/// [Fact], exactly as it does in WebRTCme.DeviceTests: it runs before the runner, so its line
/// arriving without an xUnit banner after it is the signature being looked for.
/// </remarks>
public class TrivialTest
{
    [ModuleInitializer]
    internal static void Announce()
    {
        Console.Error.WriteLine("MINIMALXUNIT | assembly loaded on " + Environment.OSVersion);
        Console.Error.Flush();
    }

    [Fact]
    public void True_is_true() => Assert.True(true);
}
